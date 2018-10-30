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




            if (args.Length != 0 && args[0] == "--all")
            {
                string[] files = Directory.GetFiles(".", "*.uef", SearchOption.TopDirectoryOnly);

                foreach (var fileName in files)
                {
                    Console.WriteLine("Processing: " + fileName);
                    var outputFileName = fileName.Split('.')[1].Replace(@"\", string.Empty);
                    Console.WriteLine(outputFileName);
                    var ueffile = File.ReadAllBytes(fileName);
                    var decompresseduef = Decompress(ueffile);
                    var uefChunkCollection = ParseUEF.Parse(decompresseduef);
                    BuildWAV.Generate(uefChunkCollection, outputFileName, 1200,false);
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
                ChunkCollection = ParseUEF.Parse(decompressed);
                var MaVer = ParseUEF.MajorVersion(decompressed);
                var MiVer = ParseUEF.MinorVersion(decompressed);
            }
            catch
            {
                Console.WriteLine("Something went wrong during parsing UEF file. It seems to be corrupt");
            }


                foreach(var chunk in ChunkCollection)
                {
                    
                    

                }

            try
            {
                var MinorVer = ParseUEF.MajorVersion(decompressed);
                var MajorVer = ParseUEF.MinorVersion(decompressed);
                BuildWAV.Generate(ChunkCollection, args[0].Split('.')[0], 1200,false);
            }
            catch (Exception ex)
            {
                var x = ex;
                Console.WriteLine("Something went wrong during WAV file generation.");
            }



            
            

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

