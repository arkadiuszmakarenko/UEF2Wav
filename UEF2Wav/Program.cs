using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace UEF2Wav
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] file = File.ReadAllBytes("C:\\UEF\\DeadOrAlive_BE.uef");
            byte[] decompressed = Decompress(file);
            Console.WriteLine(file.Length);
            Console.WriteLine(decompressed.Length);
            File.WriteAllBytes("C:\\UEF\\DeadOrAlive_BE.dat", decompressed);
            ParseUEF(decompressed);
        }

        public class ChunkDataItem
        {
            public byte[] Header { get; set; }
            public byte[] Data { get; set; }
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

    }
}
