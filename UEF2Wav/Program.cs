using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;


namespace UEF2Wav
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] file = File.ReadAllBytes("C:\\UEF\\Commando_E.uef");
            byte[] decompressed = Decompress(file);
            var ChunkCollection = ParseUEF(decompressed);
            BuildWav(ChunkCollection);
        }

        static List<ChunkDataItem> ParseUEF(byte[] uefFile)
        { 
            int chunkHeaderStartIndex = 12;
            List<ChunkDataItem> ChunkData = new List<ChunkDataItem>();
            while (chunkHeaderStartIndex < uefFile.Length)
            {
                var Lenght = new byte[4];
                int LenghtInt;
                var chunkObj = new ChunkDataItem();
                chunkObj.Header = new byte[2];
                Array.Copy(uefFile, chunkHeaderStartIndex, chunkObj.Header, 0, 2);
                Array.Copy(uefFile, chunkHeaderStartIndex+2, Lenght, 0, 4);
                LenghtInt = BitConverter.ToInt32(Lenght, 0);
                chunkObj.Data = new byte[LenghtInt];
                Array.Copy(uefFile, chunkHeaderStartIndex + 6, chunkObj.Data, 0, LenghtInt);
                chunkHeaderStartIndex = chunkHeaderStartIndex + LenghtInt + 6;
                ChunkData.Add(chunkObj);
                
            }
            return ChunkData;
        }

        public class ChunkDataItem
        {
            public byte[] Header { get; set; }
            public byte[] Data { get; set; }
        }

        static byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip),
                CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }


        static void BuildWav(List<ChunkDataItem> ChunksList)
        {
            string format = BitConverter.IsLittleEndian
                                ? "x{1:X2}{0:X2}"
                                : "x{0:X2}{1:X2}";

            int formatChunkSize = 16;
            int headerSize = 8;
            short formatType = 1;
            short tracks = 1;
            int samplesPerSecond = 44100;
            short bitsPerSample = 16;
            short frameSize = (short)(tracks * ((bitsPerSample) / 8));
            int bytesPerSecond = samplesPerSecond * frameSize;
            int waveSize = 4;
            int sampleCount = 0;

            var bit0Array = GenerateWave.bit0(samplesPerSecond);
            var bit1Array = GenerateWave.bit1(samplesPerSecond);
            var highWaveArray = GenerateWave.highWave(samplesPerSecond);
            

            MemoryStream dataStream = new MemoryStream();
            BinaryWriter dataStreamWriter = new BinaryWriter(dataStream);

            foreach (var ChunkDataItem in ChunksList)
            {

                
                var headerValue = String.Format(format, ChunkDataItem.Header[0], ChunkDataItem.Header[1]);

       

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

                  

                    
                       // for (int i = 0; i < ChunkDataItem.Data.Length; i++)
                       foreach (bool bit in deliveryBitArray)
                        {

                            if (bit == false) //0
                            {
                                foreach (var sample in bit0Array)
                                {
                                    dataStreamWriter.Write(sample);
                                    sampleCount = sampleCount + 1;
                                }

                            }
                            else // 1
                            {
                                foreach (var sample in bit1Array)
                                {
                                    dataStreamWriter.Write(sample);
                                    sampleCount = sampleCount + 1;
                                }

                            }
                   
                        



                    }

                }
                }

                if (headerValue == "x0112")
                {
                    //var n = BitConverter.ToInt16(ChunkDataItem.Data, 0);
                    //var sec = 1 / (2 * n * 1200);

                    for (int g = 0; g <= 44100; g++)
                    {

                    dataStreamWriter.Write(0);
                    sampleCount = sampleCount + 1;
                    dataStreamWriter.Write(0);
                    sampleCount = sampleCount + 1;
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
                            sampleCount = sampleCount + 1;
                        }
                    }



                }


            }


            //For each byte in array
            //
            //{
            //    double theta = frequency * TAU / (double)samplesPerSecond;
            //
            //    // 'volume' is UInt16 with range 0 thru Uint16.MaxValue ( = 65 535)
            //    // we need 'amp' to have the range of 0 thru Int16.MaxValue ( = 32 767)
            //    double amp = volume >> 2; // so we simply set amp = volume / 2
            //    for (int step = 0; step < samples; step++)
            //    {
            //        short s = (short)(amp * Math.Sin((theta * (double)step) + Math.PI)); //start from 180 pulse
            //        dataStreamWriter.Write(s);
            //        sampleCount = sampleCount + 1;
            //    }
            //}



            int dataChunkSize = sampleCount * frameSize;
            int fileSize = waveSize + headerSize + formatChunkSize + headerSize + dataChunkSize;

            FileStream headerStream = new FileStream("C://UEF//Commando_E.wav", FileMode.Create);
            //MemoryStream headerStream = new MemoryStream();
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
            //mStrm.Seek(0, SeekOrigin.Begin);




            // headerStream.CopyTo(mStrm);
            dataStream.Seek(0, SeekOrigin.Begin);
            dataStream.CopyTo(headerStream);
            dataStreamWriter.Close();
            dataStream.Close();

            headerStreamWriter.Close();
            headerStream.Close();


        }

    }
}
