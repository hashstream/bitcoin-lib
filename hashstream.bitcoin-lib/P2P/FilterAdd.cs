using hashstream.bitcoin_lib.BlockChain;
using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class FilterAdd : IStreamable, ICommand
    {
        public VarInt ElementCount { get; set; }
        public byte[] Elements { get; set; }

        public string Command => "filteradd";

        public void ReadFromPayload(byte[] data, int offset)
        {
            ElementCount = new VarInt(0);
            ElementCount.ReadFromPayload(data, offset);

            Elements = new byte[ElementCount];
            Buffer.BlockCopy(Elements, offset + ElementCount, Elements, 0, ElementCount);
        }

        public byte[] ToArray()
        {
            var ret = new byte[ElementCount.Size + ElementCount];

            var ec = ElementCount.ToArray();
            Buffer.BlockCopy(ec, 0, ret, 0, ec.Length);

            Buffer.BlockCopy(Elements, 0, ret, ec.Length, Elements.Length);

            return ret;
        }
    }
}
