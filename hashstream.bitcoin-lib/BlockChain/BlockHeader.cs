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

        public static int Size => 80;

        public Hash Hash => new Hash(ToArray().SHA256d());

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
    }
}