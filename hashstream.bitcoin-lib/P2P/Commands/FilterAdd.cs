using hashstream.bitcoin_lib.BlockChain;
using System;

namespace hashstream.bitcoin_lib.P2P
{
    public class FilterAdd : IStreamable, ICommand
    {
        public VarInt ElementCount => Elements?.Length;
        public byte[] Elements { get; set; } = new byte[0];

        public string Command => "filteradd";

        public int Size => ElementCount.Size + ElementCount;
        
#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out VarInt tElementCount)
                .ReadAndSlice(tElementCount, out byte[] tElements);

            Elements = tElements;

            return next.Slice(tElementCount);
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(ElementCount)
                 .WriteAndSlice(Elements);
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
            var roffset = offset;
            var ec = data.ReadFromBuffer<VarInt>(ref roffset);

            Elements = new byte[ec];
            Array.Copy(data, roffset, Elements, 0, ec);

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];

            ret.CopyAndIncr(ElementCount.ToArray(), ref woffset);
            ret.CopyAndIncr(Elements, woffset);

            return ret;
        }
#endif
    }
}
