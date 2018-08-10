using hashstream.bitcoin_lib.BlockChain;
using System;

namespace hashstream.bitcoin_lib.P2P
{
    public class MerkleBlock : IStreamable
    {
        public BlockHeader BlockHeader { get; set; }
        public UInt32 Transactions { get; set; }
        public VarInt HashCount { get; set; }
        public Hash[] Hashes { get; set; }
        public VarInt FlagByteCount { get; set; }
        public byte[] Flags { get; set; }

        public int Size => BlockHeader.Size + 4 + HashCount.Size + (Hash.Size * HashCount) + FlagByteCount.Size + FlagByteCount;

        public int ReadFromPayload(byte[] data, int offset)
        {
            var roffset = offset;

            BlockHeader = data.ReadFromBuffer<BlockHeader>(ref roffset);
            Transactions = data.ReadUInt32FromBuffer(ref roffset);
            HashCount = data.ReadFromBuffer<VarInt>(ref roffset);

            Hashes = new Hash[HashCount];
            for (var x = 0; x < HashCount; x++)
            {
                Hashes[x] = data.ReadFromBuffer<Hash>(ref roffset);
            }

            FlagByteCount = data.ReadFromBuffer<VarInt>(ref roffset);

            Flags = new byte[FlagByteCount];
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

            ret.CopyAndIncr(FlagByteCount.ToArray(), ref woffset);
            ret.CopyAndIncr(Flags, woffset);

            return ret;

        }
    }
}
