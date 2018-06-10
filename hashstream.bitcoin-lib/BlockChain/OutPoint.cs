using hashstream.bitcoin_lib.P2P;
using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.BlockChain
{
    public class Outpoint : IStreamable
    {
        public Hash Hash { get; set; }
        public UInt32 Index { get; set; }

        public int Size => 36;

        public void ReadFromPayload(byte[] data, int offset)
        {
            Hash = new Hash();
            Hash.ReadFromPayload(data, offset);

            Index = BitConverter.ToUInt32(data, offset + 32);
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];
            Array.Copy(Hash.HashBytes, 0, ret, 0, Hash.HashBytes.Length);

            var id = BitConverter.GetBytes(Index);
            Array.Copy(id, 0, ret, 32, id.Length);

            return ret;
        }
    }
}
