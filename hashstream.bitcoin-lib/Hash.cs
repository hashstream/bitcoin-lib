using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib
{
    public class Hash
    {
        public byte[] HashBytes { get; set; }

        public Hash()
        {
            HashBytes = new byte[32];
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
