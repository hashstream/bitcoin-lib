using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace hashstream.bitcoin_lib
{
    public static class StreamExtensions
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

        public static async Task ReadAsyncExact(this Stream s, byte[] buf, int offset, int count)
        {
            var i_offset = 0;

            read_more:
            var rlen = await s.ReadAsync(buf, offset + i_offset, count - i_offset);
            if (rlen == 0)
                return;

            if(i_offset + rlen < count - i_offset)
            {
                i_offset += rlen;
                goto read_more;
            }
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
    }
}
