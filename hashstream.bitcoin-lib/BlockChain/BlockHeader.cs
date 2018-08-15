using hashstream.bitcoin_lib.P2P;
using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.BlockChain
{
    public class BlockHeader : IStreamable
    {
        public UInt32 Version { get; set; }
        public Hash PrevBlock { get; set; }
        public Hash MerkleRoot { get; set; }
        public UInt32 Time { get; set; }
        public UInt32 Target { get; set; }
        public UInt32 Nonce { get; set; }

        public int Size => StaticSize;

        public static int StaticSize => 80;

        public Hash Hash => new Hash(ToArray().SHA256d());

#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out UInt32 tVersion)
                .ReadAndSlice(out Hash tPrevBlock)
                .ReadAndSlice(out Hash tMerkleRoot)
                .ReadAndSlice(out UInt32 tTime)
                .ReadAndSlice(out UInt32 tTarget)
                .ReadAndSlice(out UInt32 tNonce);

            Version = tVersion;
            PrevBlock = tPrevBlock;
            MerkleRoot = tMerkleRoot;
            Time = tTime;
            Target = tTarget;
            Nonce = tNonce;

            return next;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(Version)
                .WriteAndSlice(PrevBlock)
                .WriteAndSlice(MerkleRoot)
                .WriteAndSlice(Time)
                .WriteAndSlice(Target)
                .WriteAndSlice(Nonce);
        }

        public byte[] ToArray()
        {
            var dest = new byte[Size];
            WriteToPayload(dest);
            return dest;
        }
#else
        public int ReadFromPayload(byte[] data, int offset)
        {
            var roffset = offset;

            Version = data.ReadUInt32FromBuffer(ref roffset);
            PrevBlock = data.ReadFromBuffer<Hash>(ref roffset);
            MerkleRoot = data.ReadFromBuffer<Hash>(ref roffset);
            Time = data.ReadUInt32FromBuffer(ref roffset);
            Target = data.ReadUInt32FromBuffer(ref roffset);
            Nonce = data.ReadUInt32FromBuffer(ref roffset);

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];

            ret.CopyAndIncr(BitConverter.GetBytes(Version), ref woffset);
            ret.CopyAndIncr(PrevBlock.ToArray(), ref woffset);
            ret.CopyAndIncr(MerkleRoot.ToArray(), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(Time), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(Target), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(Nonce), ref woffset);

            return ret;
        }

#endif
    }
}