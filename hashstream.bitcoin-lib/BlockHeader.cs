using hashstream.bitcoin_lib.P2P;
using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib
{
    public class BlockHeader : IStreamable
    {
        public UInt32 Version { get; set; }
        public Hash PrevBlock { get; set; }
        public Hash MerkleRoot { get; set; }
        public UInt32 Time { get; set; }
        public UInt32 Target { get; set; }
        public UInt32 Nonce { get; set; }

        public string Command => null;

        public void ReadFromPayload(byte[] data, int offset)
        {
            Version = BitConverter.ToUInt32(data, offset);
            PrevBlock = new Hash();
            PrevBlock.ReadFromPayload(data, offset + 4);
            MerkleRoot = new Hash();
            MerkleRoot.ReadFromPayload(data, offset + 36);
            Time = BitConverter.ToUInt32(data, offset + 68);
            Target = BitConverter.ToUInt32(data, offset + 72);
            Nonce = BitConverter.ToUInt32(data, offset + 76);
        }

        public byte[] ToArray()
        {
            var ret = new byte[80];

            var v = BitConverter.GetBytes(Version);
            Buffer.BlockCopy(v, 0, ret, 0, v.Length);

            var pb = PrevBlock.ToArray();
            Buffer.BlockCopy(pb, 0, ret, 4, pb.Length);

            var mr = MerkleRoot.ToArray();
            Buffer.BlockCopy(mr, 0, ret, 36, mr.Length);

            var t = BitConverter.GetBytes(Time);
            Buffer.BlockCopy(t, 0, ret, 68, t.Length);

            var nb = BitConverter.GetBytes(Target);
            Buffer.BlockCopy(nb, 0, ret, 72, nb.Length);

            var nn = BitConverter.GetBytes(Nonce);
            Buffer.BlockCopy(nn, 0, ret, 76, nn.Length);

            return ret;
        }
    }
}
