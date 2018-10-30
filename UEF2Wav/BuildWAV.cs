using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace UEF2Wav
{
   public class BuildWAV
    {

       public static void Generate(List<ChunkDataItem> ChunksList, string fileName, int baud, bool invertParity)
        {

            // HashSet to check what chunks are used in collection

            HashSet<string> ChunkUniqueList = new HashSet<string>();

            string format = BitConverter.IsLittleEndian
                                ? "x{1:X2}{0:X2}"
                                : "x{0:X2}{1:X2}";

            int formatChunkSize = 16;
            int headerSize = 8;
            short formatType = 1;
            short tracks = 1;
            int samplesPerSecond = 44100;
            var samplesPerCycle = 36;
            short bitsPerSample = 16;
            short frameSize = (short)(tracks * ((bitsPerSample) / 8));
            int bytesPerSecond = samplesPerSecond * frameSize;
            int waveSize = 4;

            var bit0Array = GenerateSinWave.bit0(samplesPerSecond, baud);
            var bit1Array = GenerateSinWave.bit1(samplesPerSecond, baud);
            var highWaveArray = GenerateSinWave.highWave(samplesPerSecond, baud);


            MemoryStream dataStream = new MemoryStream();
            BinaryWriter dataStreamWriter = new BinaryWriter(dataStream);

            foreach (var ChunkDataItem in ChunksList)
            {


                var headerValue = String.Format(format, ChunkDataItem.Header[0], ChunkDataItem.Header[1]);
                Console.WriteLine(headerValue);
                ChunkUniqueList.Add(headerValue);


                if (headerValue == "x0100") //Chunk &0100 - implicit start/stop bit tape data block
                {



                    foreach (byte myByte in ChunkDataItem.Data)
                    {
                        bool[] deliveryBitArray = new bool[10];

                        deliveryBitArray[0] = false;
                        deliveryBitArray[1] = (myByte & (1 << 0)) != 0;
                        deliveryBitArray[2] = (myByte & (1 << 1)) != 0;
                        deliveryBitArray[3] = (myByte & (1 << 2)) != 0;
                        deliveryBitArray[4] = (myByte & (1 << 3)) != 0;
                        deliveryBitArray[5] = (myByte & (1 << 4)) != 0;
                        deliveryBitArray[6] = (myByte & (1 << 5)) != 0;
                        deliveryBitArray[7] = (myByte & (1 << 6)) != 0;
                        deliveryBitArray[8] = (myByte & (1 << 7)) != 0;
                        deliveryBitArray[9] = true;

                        foreach (bool bit in deliveryBitArray)
                        {
                            if (bit == false) //process 0
                            {
                                foreach (var sample in bit0Array)
                                {
                                    dataStreamWriter.Write(sample);
                                }
                            }
                            else // 1
                            {
                                foreach (var sample in bit1Array)
                                {
                                    dataStreamWriter.Write(sample);
                                    
                                }
                            }
                        }
                        

                    }
                }



                if (headerValue == "x0110") //Chunk &0110 - carrier tone (previously referred to as 'high tone')
                {
                    int cyclesOfCarrierTone = BitConverter.ToInt16(ChunkDataItem.Data, 0);

                    for (var count = 0; count < cyclesOfCarrierTone; count++)
                    {
                        foreach (var sample in highWaveArray)
                        {
                            dataStreamWriter.Write(sample);
                        }
                    }
                }

                if (headerValue == "x0111") //Chunk &0111 - carrier tone with dummy byte
                {
                    byte[] beforeTone = new byte[2];
                    byte[] afterTone = new byte[2];

                    Array.Copy(ChunkDataItem.Data, 0, beforeTone, 0, 2);
                    Array.Copy(ChunkDataItem.Data, 2, afterTone, 0, 2);



                    int beforeGapCycles = BitConverter.ToInt16(beforeTone, 0);
                    int afterGapCycles = BitConverter.ToInt16(afterTone, 0);

                    for (var count = 0; count < beforeGapCycles; count++)
                    {
                        foreach (var sample in highWaveArray)
                        {
                            dataStreamWriter.Write(sample);
                        }
                    }

                    // dummy byte 0, 0, 1, 0, 1, 0, 1, 0, 1, 1

                    bool[] dummyByteArray = new bool[] { false, false, true, false, true, false, true, false, true, true };



                    foreach (bool bit in dummyByteArray)
                    {
                        if (bit == false) //process 0
                        {
                            foreach (var sample in bit0Array)
                            {
                                dataStreamWriter.Write(sample);
                            }
                        }
                        else // 1
                        {
                            foreach (var sample in bit1Array)
                            {
                                dataStreamWriter.Write(sample);
                            }
                        }



                        for (var count = 0; count < afterGapCycles; count++)
                        {
                            foreach (var sample in highWaveArray)
                            {
                                dataStreamWriter.Write(sample);
                            }
                        }


                    }

                }



                if (headerValue == "x0112")
                {
                    var n = BitConverter.ToInt16(ChunkDataItem.Data, 0);
                    double sec = 1 / (2 *  (double)n * (double)baud);
                    var testGap = samplesPerSecond * sec;
                    var gapcycles = samplesPerCycle * n;

                    for (int g = 0; g <= gapcycles; g++)
                    {
                        dataStreamWriter.Write(0);
                    }
                }

                if (headerValue == "x0114") //security waves
                {

                    var cycles = BitConverter.ToUInt32(ChunkDataItem.Data, 0) & 0x00ffffff;


                   for (var count = 0; count < cycles; count++)
                        {
                            foreach (var sample in highWaveArray)
                            {
                                dataStreamWriter.Write(sample);
                            }
                        }
                }

                if (headerValue == "x0116") //Chunk &0116 - floating point gap
                {

                    double sec = Math.Ceiling(BitConverter.ToSingle(ChunkDataItem.Data, 0));
                     var gapSamples = samplesPerSecond * sec;
                
                    

                    for (int g = 0; g <= gapSamples; g++)
                    {
                        dataStreamWriter.Write(0);
                    }
                }

            }





            int dataChunkSize = (int)dataStream.Length * frameSize;
            int fileSize = waveSize + headerSize + formatChunkSize + headerSize + dataChunkSize;
            var x = dataStream.Length ;
            FileStream headerStream = new FileStream(fileName + ".wav", FileMode.Create);

            BinaryWriter headerStreamWriter = new BinaryWriter(headerStream);


            headerStreamWriter.Write(0x46464952); // = encoding.GetBytes("RIFF")
            headerStreamWriter.Write(fileSize);
            headerStreamWriter.Write(0x45564157); // = encoding.GetBytes("WAVE")
            headerStreamWriter.Write(0x20746D66); // = encoding.GetBytes("fmt ")
            headerStreamWriter.Write(formatChunkSize);
            headerStreamWriter.Write(formatType);
            headerStreamWriter.Write(tracks);
            headerStreamWriter.Write(samplesPerSecond);
            headerStreamWriter.Write(bytesPerSecond); // bytesPerSecond
            headerStreamWriter.Write(frameSize);
            headerStreamWriter.Write(bitsPerSample);
            headerStreamWriter.Write(0x61746164); // = encoding.GetBytes("data")
            headerStreamWriter.Write(dataChunkSize);


            dataStream.Seek(0, SeekOrigin.Begin);
            dataStream.CopyTo(headerStream);
            dataStreamWriter.Close();
            dataStream.Close();

            headerStreamWriter.Close();
            headerStream.Close();



        }

    }
}
