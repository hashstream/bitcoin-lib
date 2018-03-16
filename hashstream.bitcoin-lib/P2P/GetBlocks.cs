using System;
using System.IO;

namespace hashstream.bitcoin_lib.P2P
{
    public class GetBlocks : IStreamable
    {
        public UInt32 Version { get; set; } = 700015;
        public VarInt HashCount { get; set; }
        public Hash[] Hashes { get; set; }
        public Hash StopHash { get; set; }

        public string Command => "getblocks";

        public void ReadFromPayload(byte[] data, int offset)
        {
            Version = BitConverter.ToUInt32(data, offset);
            HashCount = new VarInt(0);
            HashCount.ReadFromPayload(data, 4);

            Hashes = new Hash[(int)HashCount];

            for (var x = 0; x < (int)HashCount; x++)
            {
                var ch = new Hash() { HashBytes = new byte[32] };
                Buffer.BlockCopy(data, offset + 4 + HashCount.Size + (ch.HashBytes.Length * x), ch.HashBytes, 0, ch.HashBytes.Length);

                Hashes[x] = ch;
            }

            StopHash = new Hash() { HashBytes = new byte[32] };
            Buffer.BlockCopy(data, offset + 4 + HashCount.Size + ((int)HashCount * 32), StopHash.HashBytes, 0, StopHash.HashBytes.Length);
        }

        public byte[] ToArray()
        {
            using (var ms = new MemoryStream())
            {
                var v = BitConverter.GetBytes(Version);
                ms.Write(v, 0, v.Length);

                var hc = HashCount.ToArray();
                ms.Write(hc, 0, hc.Length);

                for (var x = 0; x < (int)HashCount; x++)
                {
                    var xh = Hashes[x].HashBytes;
                    ms.Write(xh, 0, xh.Length);
                }

                ms.Write(StopHash.HashBytes, 0, StopHash.HashBytes.Length);

                return ms.ToArray();
            }
        }
    }
}
