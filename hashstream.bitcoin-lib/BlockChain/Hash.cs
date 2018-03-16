using hashstream.bitcoin_lib.P2P;
using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.BlockChain
{
    public class Hash : IStreamable
    {
        public byte[] HashBytes { get; set; }

        public int Size => 32;

        public Hash()
        {
            HashBytes = new byte[Size];
        }

        public override string ToString()
        {
            return BitConverter.ToString(HashBytes).Replace("-", string.Empty).ToLower();
        }

        public void ReadFromPayload(byte[] data, int offset)
        {
            Buffer.BlockCopy(data, offset, HashBytes, 0, HashBytes.Length);
        }

        public byte[] ToArray()
        {
            return HashBytes;
        }
    }
}
