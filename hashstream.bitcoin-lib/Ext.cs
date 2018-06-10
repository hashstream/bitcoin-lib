using System;
using System.Collections.Generic;
using System.IO;
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
    }
}
