using hashstream.bitcoin_lib.BlockChain;
using System;
using System.Collections.Generic;
using System.Text;

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
       
        public void ReadFromPayload(byte[] data, int offset)
        {
            BlockHeader = new BlockHeader();
            BlockHeader.ReadFromPayload(data, offset);

            Transactions = BitConverter.ToUInt32(data, offset + 80);
            HashCount = new VarInt(0);
            HashCount.ReadFromPayload(data, offset + 84);
            Hashes = new Hash[HashCount];

            for (var x = 0; x < HashCount; x++)
            {
                var nh = new Hash();
                nh.ReadFromPayload(data, offset + 84 + HashCount.Size + (x * 32));

                Hashes[x] = nh;
            }

            FlagByteCount = new VarInt(0);
            FlagByteCount.ReadFromPayload(data, offset + 84 + HashCount.Size + (HashCount * 32));

            Flags = new byte[FlagByteCount];
            Buffer.BlockCopy(data, offset + 84 + HashCount.Size + (HashCount * 32) + FlagByteCount.Size, Flags, 0, Flags.Length);
        }

        public byte[] ToArray()
        {
            var ret = new byte[84 + HashCount.Size + FlagByteCount.Size + FlagByteCount + (32 * HashCount)];

            var bh = BlockHeader.ToArray();
            Buffer.BlockCopy(bh, 0, ret, 0, bh.Length);

            var tr = BitConverter.GetBytes(Transactions);
            Buffer.BlockCopy(tr, 0, ret, 80, tr.Length);

            var hc = HashCount.ToArray();
            Buffer.BlockCopy(hc, 0, ret, 84, hc.Length);

            for (var x = 0; x < HashCount; x++)
            {
                var hx = Hashes[x].ToArray();
                Buffer.BlockCopy(hx, 0, ret, 84 + hc.Length + (x * 32), hx.Length);
            }

            var fb = FlagByteCount.ToArray();
            Buffer.BlockCopy(fb, 0, ret, 84 + hc.Length + (32 * HashCount), fb.Length);

            Buffer.BlockCopy(Flags, 0, ret, 84 + hc.Length + (32 * HashCount) + fb.Length, Flags.Length);

            return ret;

        }
    }
}
