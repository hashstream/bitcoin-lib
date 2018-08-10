using hashstream.bitcoin_lib.Crypto;
using hashstream.bitcoin_lib.P2P;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace hashstream.bitcoin_lib
{
    public static class Extensions
    {
        private static Dictionary<string, byte> HexTable { get; set; } = BuildHexTable();

        private static Dictionary<string, byte> BuildHexTable()
        {
            var ret = new Dictionary<string, byte>();
            for(var x = 0; x <= 255; x++)
            {
                ret.Add(x.ToString("X2"), (byte)x);
            }

            return ret;
        }

        public static byte[] FromHex(this string s)
        {
            if(s.Length % 2 != 0)
            {
                throw new Exception("Invalid hex string");
            }

            s = s.ToUpper();

            var ret = new byte[s.Length / 2];
            for(var x = 0; x < s.Length; x += 2)
            {
                ret[x / 2] = HexTable[$"{s[x]}{s[x + 1]}"];
            }

            return ret;
        }

        public static string ToHex(this byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", "").ToLower();
        }

        public static async Task<int> ReadAsyncExact(this Stream s, byte[] buf, int offset, int count)
        {
            var i_offset = 0;

            read_more:
            var rlen = await s.ReadAsync(buf, offset + i_offset, count - i_offset);
            if (rlen == 0)
                return 0;

            if(i_offset + rlen < count - i_offset)
            {
                i_offset += rlen;
                goto read_more;
            }

            return count;
        }

        public static T[] Concat<T>(this T[] a, T[] b)
        {
            var ret = new T[a.Length + b.Length];
            Array.Copy(a, ret, a.Length);
            Array.Copy(b, 0, ret, a.Length, b.Length);
            return ret;
        }

        public static byte[] SHA256d(this byte[] data)
        {
            byte[] hash = null;
            using (var sha = new SHA256Managed())
            {
                hash = sha.ComputeHash(data);
                hash = sha.ComputeHash(hash);
            }

            return hash;
        }

        public static byte[] SHA256(this byte[] data)
        {
            using (var sha = new SHA256Managed())
            {
                return sha.ComputeHash(data);
            }
        }

        public static byte[] RIPEMD160(this byte[] data)
        {
            using(var rmd = new RIPEMD160Managed())
            {
                return rmd.ComputeHash(data);
            }
        }

        public static byte[] Hash160(this byte[] data)
        {
            return data.SHA256().RIPEMD160();
        }

        public static void CopyAndIncr(this byte[] dest, byte[] src, ref int offset, bool reverse = false)
        {
            if (reverse)
            {
                Array.Reverse(src);
            }
            Array.Copy(src, 0, dest, offset, src.Length);
            offset += src.Length;
        }

        public static void CopyAndIncr(this byte[] dest, byte[] src, int offset, bool reverse = false)
        {
            if (reverse)
            {
                Array.Reverse(src);
            }
            Array.Copy(src, 0, dest, offset, src.Length);
        }

        public static T ReadFromBuffer<T>(this byte[] src, ref int offset) where T : IStreamable, new()
        {
            var ret = new T();
            offset += ret.ReadFromPayload(src, offset);

            return ret;
        }

        public static T ReadFromBuffer<T>(this byte[] src) where T : IStreamable, new()
        {
            var ret = new T();
            ret.ReadFromPayload(src, 0);

            return ret;
        }

        public static UInt16 ReadUInt16FromBuffer(this byte[] src, ref int offset)
        {
            var ret = BitConverter.ToUInt16(src, offset);
            offset += 2;

            return ret;
        }

        public static UInt32 ReadUInt32FromBuffer(this byte[] src, ref int offset)
        {
            var ret = BitConverter.ToUInt32(src, offset);
            offset += 4;

            return ret;
        }

        public static UInt64 ReadUInt64FromBuffer(this byte[] src, ref int offset)
        {
            var ret = BitConverter.ToUInt64(src, offset);
            offset += 8;

            return ret;
        }

        public static Int16 ReadInt16FromBuffer(this byte[] src, ref int offset)
        {
            var ret = BitConverter.ToInt16(src, offset);
            offset += 2;

            return ret;
        }

        public static Int32 ReadInt32FromBuffer(this byte[] src, ref int offset)
        {
            var ret = BitConverter.ToInt32(src, offset);
            offset += 4;

            return ret;
        }

        public static Int64 ReadInt64FromBuffer(this byte[] src, ref int offset)
        {
            var ret = BitConverter.ToInt64(src, offset);
            offset += 8;

            return ret;
        }

        public static IPAddress ReadIPAddressFromBuffer(this byte[] src, ref int offset)
        {
            var ip = new byte[16];
            Array.Copy(src, offset, ip, 0, 16);
            offset += 16;

            return new IPAddress(ip);
        }

        public static string ReadASCIIFromBuffer(this byte[] src, ref int offset, int len)
        {
            var ret = System.Text.Encoding.ASCII.GetString(src, offset, len);
            offset += len;

            return ret;
        }
    }
}
