using System.IO;
using System.Threading.Tasks;

namespace hashstream.bitcoin_lib
{
    public static class StreamExtensions
    {
        public static async Task ReadAsyncExact(this Stream s, byte[] buf, int offset, int count)
        {
            var i_offset = 0;

            read_more:
            var rlen = await s.ReadAsync(buf, offset + i_offset, count - i_offset);
            if(i_offset + rlen < count - i_offset)
            {
                i_offset += rlen;
                goto read_more;
            }
        }
    }
}
