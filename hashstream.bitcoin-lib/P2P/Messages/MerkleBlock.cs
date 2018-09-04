using hashstream.bitcoin_lib.BlockChain;
using System;
using System.Linq;

namespace hashstream.bitcoin_lib.P2P
{
    public class MerkleBlock : IStreamable
    {
        public BlockHeader BlockHeader { get; set; }
        public UInt32 Transactions { get; set; }
        public VarInt HashCount => Hashes?.Length;
        public Hash[] Hashes { get; set; }
        public VarInt FlagBytes => Flags?.Length;
        public byte[] Flags { get; set; }

        public int Size => BlockHeader.Size + 4 + HashCount.Size + Hashes.Sum(a => a.Size) + FlagBytes.Size + FlagBytes;

#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out BlockHeader tHeader)
                .ReadAndSlice(out UInt32 tTransactions)
                .ReadAndSlice(out VarInt tHashCount)
                .ReadAndSlice(tHashCount, out Hash[] tHashes)
                .ReadAndSlice(out VarInt tFlagBytes)
                .ReadAndSlice(tFlagBytes, out byte[] tFlags);

            BlockHeader = tHeader;
            Transactions = tTransactions;
            Hashes = tHashes;
            Flags = tFlags;

            return next;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(BlockHeader)
                .WriteAndSlice(Transactions)
                .WriteAndSlice(HashCount)
                .WriteAndSlice(Hashes)
                .WriteAndSlice(FlagBytes)
                .WriteAndSlice(Flags);
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

            BlockHeader = data.ReadFromBuffer<BlockHeader>(ref roffset);
            Transactions = data.ReadUInt32FromBuffer(ref roffset);
            var hc = data.ReadFromBuffer<VarInt>(ref roffset);

            Hashes = new Hash[hc];
            for (var x = 0; x < hc; x++)
            {
                Hashes[x] = data.ReadFromBuffer<Hash>(ref roffset);
            }

            var fbc = data.ReadFromBuffer<VarInt>(ref roffset);

            Flags = new byte[fbc];
            Array.Copy(data, roffset, Flags, 0, Flags.Length);

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];

            ret.CopyAndIncr(BlockHeader.ToArray(), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(Transactions), ref woffset);
            ret.CopyAndIncr(HashCount.ToArray(), ref woffset);

            foreach(var h in Hashes)
            {
                ret.CopyAndIncr(h.ToArray(), ref woffset);
            }

            ret.CopyAndIncr(FlagBytes.ToArray(), ref woffset);
            ret.CopyAndIncr(Flags, woffset);

            return ret;

        }
#endif
    }
}
