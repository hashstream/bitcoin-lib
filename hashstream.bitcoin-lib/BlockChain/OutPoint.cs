using hashstream.bitcoin_lib.P2P;
using System;

namespace hashstream.bitcoin_lib.BlockChain
{
    public class Outpoint : IStreamable
    {
        public Hash Hash { get; set; }
        public UInt32 Index { get; set; }

        public int Size => StaticSize;

        public static int StaticSize => Hash.StaticSize + 4;

#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out Hash tHash)
                .ReadAndSlice(out UInt32 tIndex);

            Hash = tHash;
            Index = tIndex;

            return next;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(Hash)
                .WriteAndSlice(Index);
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];
            WriteToPayload(ret);
            return ret;
        }
#else
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

            ret.CopyAndIncr(Hash.ToArray(), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(Index), ref woffset);

            return ret;
        }
#endif
    }
}
