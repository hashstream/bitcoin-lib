using hashstream.bitcoin_lib.BlockChain;
using System;

namespace hashstream.bitcoin_lib.P2P
{
    public class FilterAdd : IStreamable, ICommand
    {
        public VarInt ElementCount { get; set; }
        public byte[] Elements { get; set; }

        public string Command => "filteradd";

        public int Size => ElementCount.Size + ElementCount;

        public int ReadFromPayload(byte[] data, int offset)
        {
            var roffset = offset;
            ElementCount = data.ReadFromBuffer<VarInt>(ref roffset);

            Elements = new byte[ElementCount];
            Array.Copy(data, roffset, Elements, 0, ElementCount);

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
    }
}
