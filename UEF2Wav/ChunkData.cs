using System;
using System.Collections.Generic;
using System.Text;

namespace UEF2Wav
{
    public class ChunkDataItem
    {
        public byte[] Header { get; set; }
        public byte[] Data { get; set; }
    }
}
