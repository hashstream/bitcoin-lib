using hashstream.bitcoin_lib.P2P;
using System;

namespace hashstream.bitcoin_lib.BlockChain
{
    public class Outpoint : IStreamable
    {
        public Hash Hash { get; set; }
        public UInt32 Index { get; set; }

        public static int Size => Hash.Size + 4;

        public int ReadFromPayload(byte[] data, int offset)
        {
            var roffset = offset;

            Hash = data.ReadFromBuffer<Hash>(ref roffset);
            Index = data.ReadUInt32FromBuffer(ref roffset);

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];

            ret.CopyAndIncr(Hash.NetworkHashBytes, ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(Index), ref woffset);

            return ret;
        }
    }
}
