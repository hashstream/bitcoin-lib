using hashstream.bitcoin_lib.BlockChain;
using System;

namespace hashstream.bitcoin_lib.P2P
{
    public class Headers : IStreamable, ICommand
    {
        public VarInt Count => Header?.Length;
        public BlockHeader[] Header { get; set; } = new BlockHeader[0];

        public string Command => "headers";

        public int Size => Count.Size + (BlockHeader.StaticSize * Count);

#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out VarInt tCount)
                .ReadAndSlice(tCount, out BlockHeader[] tHeaders);
            
            Header = tHeaders;

            return next;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(Count)
                .WriteAndSlice(Header);
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
            var roffset = offset;

            var bhc = data.ReadFromBuffer<VarInt>(ref roffset);

            Header = new BlockHeader[bhc];
            for (var x = 0; x < bhc; x++)
            {
                Header[x] = data.ReadFromBuffer<BlockHeader>(ref roffset);
            }

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];

            ret.CopyAndIncr(Count.ToArray(), ref woffset);

            foreach(var header in Header)
            {
                ret.CopyAndIncr(header.ToArray(), ref woffset);
            }

            return ret;
        }
#endif
    }
}
