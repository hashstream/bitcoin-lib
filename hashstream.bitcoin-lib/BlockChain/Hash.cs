using hashstream.bitcoin_lib.P2P;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static Hash Empty()
        {
            return new Hash();
        }

        public Hash(byte[] h)
        {
            if (h.Length != Size)
            {
                throw new Exception($"Invalid hash length {h.Length} != {Size}");
            }

            HashBytes = h;
        }

        public Hash(string h)
        {
            if (h.Length != Size * 2)
            {
                throw new Exception($"Invalid hash length {h.Length} != {Size}");
            }

            HashBytes = h.FromHex();
        }

        public override string ToString()
        {
            return BitConverter.ToString(HashBytes).Replace("-", string.Empty).ToLower();
        }

        public void ReadFromPayload(byte[] data, int offset)
        {
            Array.Copy(data, offset, HashBytes, 0, HashBytes.Length);
            HashBytes = HashBytes.Reverse().ToArray();
        }

        public byte[] ToArray()
        {
            return HashBytes;
        }

        public static implicit operator Hash(byte[] b)
        {
            return new Hash(b);
        }

        public static implicit operator Hash(string b)
        {
            return new Hash(b);
        }

        public static implicit operator string(Hash b)
        {
            return b.ToString();
        }

        public static bool operator ==(Hash h, string s)
        {
            return new Hash(s) == h;
        }

        public static bool operator !=(Hash h, string s)
        {
            return new Hash(s) != h;
        }
    }
}
