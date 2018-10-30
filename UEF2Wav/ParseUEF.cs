using System;
using System.Collections;
using System.Collections.Generic;
using UEF2Wav;

namespace UEF2Wav
{   public class ParseUEF
    {
         public static List<ChunkDataItem> Parse(byte[] uefFile)
        {
            int chunkHeaderStartIndex = 12;

            List<ChunkDataItem> ChunkData = new List<ChunkDataItem>();
            while (chunkHeaderStartIndex < uefFile.Length)
            {
                var Lenght = new byte[4];
                //int LenghtInt;
                var chunkObj = new ChunkDataItem();
                chunkObj.Header = new byte[2];
                Array.Copy(uefFile, chunkHeaderStartIndex, chunkObj.Header, 0, 2);
                Array.Copy(uefFile, chunkHeaderStartIndex + 2, Lenght, 0, 4);

                var LenghtInt = BitConverter.ToUInt16(Lenght, 0);



                         string format = BitConverter.IsLittleEndian
                                ? "x{1:X2}{0:X2}"
                                : "x{0:X2}{1:X2}";

                                 var headerValue = String.Format(format, chunkObj.Header[0], chunkObj.Header[1]);

                // Console.WriteLine(headerValue);
                chunkObj.Data = new byte[LenghtInt];
                Array.Copy(uefFile, chunkHeaderStartIndex + 6, chunkObj.Data, 0, LenghtInt);
                chunkHeaderStartIndex = chunkHeaderStartIndex + LenghtInt + 6;
                ChunkData.Add(chunkObj);

            }
            return ChunkData;
        }

        public static int MajorVersion(byte[] uefFile)
        {
            return (int)uefFile[11];
        }

            public static int MinorVersion(byte[] uefFile)
        {
            return (int)uefFile[10];
        }

    }
}