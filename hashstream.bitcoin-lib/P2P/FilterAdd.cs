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
            Array.Copy(Elements, offset + ElementCount, Elements, 0, ElementCount);
        }

        public byte[] ToArray()
        {
            var ret = new byte[ElementCount.Size + ElementCount];

            var ec = ElementCount.ToArray();
            Array.Copy(ec, 0, ret, 0, ec.Length);

            Array.Copy(Elements, 0, ret, ec.Length, Elements.Length);

            return ret;
        }
    }
}
