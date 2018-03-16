using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class Inv : IStreamable
    {
        public VarInt Count { get; set; }
        public Inventory[] Inventory { get; set; }

        public string Command => "inv";

        public void ReadFromPayload(byte[] data, int offset)
        {
            Count = new VarInt(0);
            Count.ReadFromPayload(data, offset);
            Inventory = new Inventory[(int)Count];

            //read all
            for (var x = 0; x < (int)Count; x++)
            {
                var ni = new Inventory();
                ni.ReadFromPayload(data, offset + Count.Size + (36 * x));

                Inventory[x] = ni;
            }
        }

        public byte[] ToArray()
        {
            var ret = new byte[8 + (36 * Inventory.Length)];
            var c = Count.ToArray();
            Buffer.BlockCopy(c, 0, ret, 0, Count.Size);

            for (var x = 0; x < (int)Count; x++)
            {
                var ni = Inventory[x].ToArray();
                Buffer.BlockCopy(ni, 0, ret, Count.Size + (36 * x), ni.Length);
            }

            return ret;
        }
    }

}
