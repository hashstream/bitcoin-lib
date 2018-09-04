using hashstream.bitcoin_lib.P2P;
using System;
using System.Linq;

namespace hashstream.bitcoin_lib.BlockChain
{
    public class Hash : IStreamable, IEquatable<Hash>
    {
        protected byte[] HashBytes { get; set; }

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

        public int Size => HashBytes != null ? HashBytes.Length : 0;

        public Hash()
        {
            HashBytes = new byte[32];
        }

        public static Hash Empty()
        {
            return new Hash();
        }

        public Hash(byte[] h)
        {
            HashBytes = h;
            Array.Reverse(HashBytes);
        }

        public Hash(string h)
        {
            if (h.Length % 2 != 0)
            {
                throw new Exception($"Invalid hash length {h.Length}");
            }

            HashBytes = h.FromHex();
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
        public int ReadFromPayload(byte[] data, int offset = 0)
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

        public override string ToString() => HashBytes.ToHex();

        public override int GetHashCode() => ToString().GetHashCode();
        
        public static implicit operator Hash(byte[] b) => new Hash(b);

        public static implicit operator Hash(string b) => new Hash(b);

        public static implicit operator string(Hash b) => b.ToString();

        public static bool operator ==(Hash a, Hash b) => a.Equals(b);

        public static bool operator !=(Hash a, Hash b) => !a.Equals(b);

        public bool Equals(Hash other)
        {
            if (other.HashBytes.Length != HashBytes.Length)
            {
                return false;
            }

            for (var x = 0; x < HashBytes.Length; x++)
            {
                if (HashBytes[x] != other.HashBytes[x])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
