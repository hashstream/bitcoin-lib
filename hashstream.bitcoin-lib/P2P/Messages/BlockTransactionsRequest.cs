using hashstream.bitcoin_lib.BlockChain;
using System;
using System.Linq;

namespace hashstream.bitcoin_lib.P2P
{
    public class BlockTransactionsRequest : IStreamable
    {
        public Hash BlockHash { get; set; }
        public VarInt IndexsCount => Indexes?.Length;
        public VarInt[] Indexes { get; set; }

        public int Size => BlockHash.Size + IndexsCount.Size + Indexes.Sum(a => a.Size);


#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out Hash tBlockHash)
                .ReadAndSlice(out VarInt tIndexCount)
                .ReadAndSlice(tIndexCount, out VarInt[] tIndex);

            BlockHash = tBlockHash;
            Indexes = tIndex;

            return next;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(BlockHash)
                .WriteAndSlice(IndexsCount)
                .WriteAndSlice(Indexes);
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];
            WriteToPayload(ret);

            return ret;
        }
#else
        public int ReadFromPayload(byte[] data, int offset = 0)
        {
            var roffset = offset;

            BlockHash = data.ReadFromBuffer<Hash>(ref roffset);

            var tIndexCount = data.ReadFromBuffer<VarInt>(ref roffset);
            Indexes = data.ReadFromBuffer<VarInt>(tIndexCount, ref roffset);

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];

            ret.CopyAndIncr(BlockHash, ref woffset);
            ret.CopyAndIncr(IndexsCount, ref woffset);
            ret.CopyAndIncr(Indexes, ref woffset);

            return ret;
        }
#endif
    }
}
