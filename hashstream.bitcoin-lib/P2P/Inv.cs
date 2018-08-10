using hashstream.bitcoin_lib.BlockChain;
using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class Inv : IStreamable, ICommand
    {
        public VarInt Count { get; set; }
        public Inventory[] Inventory { get; set; }

        public string Command => "inv";

        public int Size => Count.Size + (P2P.Inventory.Size * Count);

        public int ReadFromPayload(byte[] data, int offset)
        {
            var roffset = offset;
            Count = data.ReadFromBuffer<VarInt>(ref roffset);

            Inventory = new Inventory[Count];
            for (var x = 0; x < Count; x++)
            {
                Inventory[x] = data.ReadFromBuffer<Inventory>(ref roffset);
            }

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];

            ret.CopyAndIncr(Count.ToArray(), ref woffset);

            foreach(var inv in Inventory)
            {
                ret.CopyAndIncr(inv.ToArray(), ref woffset);
            }

            return ret;
        }
    }

}