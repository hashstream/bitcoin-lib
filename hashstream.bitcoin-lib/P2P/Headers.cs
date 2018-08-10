using hashstream.bitcoin_lib.BlockChain;
using System;

namespace hashstream.bitcoin_lib.P2P
{
    public class Headers : IStreamable, ICommand
    {
        public VarInt Count { get; set; }
        public BlockHeader[] Header { get; set; }

        public string Command => "headers";

        public int Size => Count.Size + (BlockHeader.Size * Count);

        public int ReadFromPayload(byte[] data, int offset)
        {
            var roffset = offset;

            Count = data.ReadFromBuffer<VarInt>(ref roffset);

            Header = new BlockHeader[Count];
            for (var x = 0; x < Count; x++)
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
    }
}
