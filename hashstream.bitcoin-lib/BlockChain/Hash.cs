using hashstream.bitcoin_lib.P2P;
using System;
using System.Linq;

namespace hashstream.bitcoin_lib.BlockChain
{
    public class Hash : IStreamable
    {
        protected byte[] HashBytes { get; set; } = new byte[StaticSize];

#if NETCOREAPP2_1
        public Span<byte> NetworkHashBytes
        {
            get
            {
                var hb = new Span<byte>(new byte[Size]);
                HashBytes.CopyTo(hb);
                hb.Reverse();
                return hb;
            }
        }
#else
        public byte[] NetworkHashBytes => HashBytes.Reverse().ToArray();
#endif

        public int Size => StaticSize;

        public static int StaticSize => 32;

        public Hash()
        {

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
            Array.Reverse(HashBytes);
        }

        public Hash(string h)
        {
            if (h.Length != Size * 2)
            {
                throw new Exception($"Invalid hash length {h.Length} != {Size}");
            }

            HashBytes = h.FromHex();
            Array.Reverse(HashBytes);
        }

#if NETCOREAPP2_1
        public Hash(Span<byte> span)
        {
            HashBytes = span.ToArray();
            Array.Reverse(HashBytes);
        }
        
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            HashBytes = data.Slice(0, Size).ToArray();
            Array.Reverse(HashBytes);
            return data.Slice(Size);
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            NetworkHashBytes.CopyTo(dest);
            return dest.Slice(Size);
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];
            WriteToPayload(ret);
            return ret;
        }
#else
        public int ReadFromPayload(byte[] data, int offset)
        {
            Array.Copy(data, offset, HashBytes, 0, HashBytes.Length);
            HashBytes = HashBytes.Reverse().ToArray();

            return Size;
        }

        public byte[] ToArray()
        {
            return NetworkHashBytes;
        }
#endif

        public override string ToString()
        {
            return HashBytes.ToHex();
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
    }
}
