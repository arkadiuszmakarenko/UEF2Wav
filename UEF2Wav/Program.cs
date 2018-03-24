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
            if (args.Length == 0 || args[0] == "--h" || args[0] == "--help")

            {
                Console.WriteLine("Usage. Convert all files in folder : UEF2WAV --all");
                Console.WriteLine("Convert one file : UEF2WAV {filename}");
                Console.WriteLine("Diplay this menu: UEF2WAV --h or --help");

            }




            if (args[0] == "--all")
            {
                string[] files = Directory.GetFiles(".", "*.uef", SearchOption.TopDirectoryOnly);

                foreach (var fileName in files)
                {
                    Console.WriteLine("Processing: " + fileName);
                    var outputFileName = fileName.Split('.')[1].Replace(@"\", string.Empty);
                    Console.WriteLine(outputFileName);
                    var ueffile = File.ReadAllBytes(fileName);
                    var decompresseduef = Decompress(ueffile);
                    var uefChunkCollection = ParseUEF(decompresseduef);
                    BuildWAV.Generate(uefChunkCollection, outputFileName, 1200);
                    uefChunkCollection = null;
                    Console.WriteLine(fileName+" completed");

                }
                Environment.Exit(0);
            }


            byte[] file = null;
            byte[] decompressed = null;
            List<ChunkDataItem> ChunkCollection = null;
            try
            {
                file = File.ReadAllBytes(args[0]);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Cannot find file with this name.");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Environment.Exit(0);
            }

            try
            {
                decompressed = Decompress(file);
            }
            catch
            {
                Console.WriteLine("Something went wrong with decompression");
                Environment.Exit(0);
            }

            try
            {
                ChunkCollection = ParseUEF(decompressed);
            }
            catch
            {
                Console.WriteLine("Something went wrong during parsing UEF file. It seems to be corrupt");
            }


            try
            {
                BuildWAV.Generate(ChunkCollection, args[0].Split('.')[0], 1200);
            }
            catch
            {
                Console.WriteLine("Something went wrong during WAV file generation.");
            }



            Console.ReadLine();

        }

        static List<ChunkDataItem> ParseUEF(byte[] uefFile)
        {
            int chunkHeaderStartIndex = 12;

            var MinorVersion = (int)uefFile[10];
         

            var MajorVersion = (int)uefFile[11];

            Console.WriteLine("Version: "+MajorVersion + "." + MinorVersion);


            List<ChunkDataItem> ChunkData = new List<ChunkDataItem>();
            while (chunkHeaderStartIndex < uefFile.Length)
            {
                var Lenght = new byte[4];
                int LenghtInt;
                var chunkObj = new ChunkDataItem();
                chunkObj.Header = new byte[2];
                Array.Copy(uefFile, chunkHeaderStartIndex, chunkObj.Header, 0, 2);
                Array.Copy(uefFile, chunkHeaderStartIndex + 2, Lenght, 0, 4);
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

