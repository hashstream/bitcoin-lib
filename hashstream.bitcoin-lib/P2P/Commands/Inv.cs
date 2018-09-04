using hashstream.bitcoin_lib.BlockChain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class Inv : IStreamable, ICommand
    {
        public VarInt Count => Inventory?.Length;
        public Inventory[] Inventory { get; set; } = new Inventory[0];

        public string Command => "inv";

        public int Size => Count.Size + Inventory.Sum(a => a.Size);

#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out VarInt tCount)
                .ReadAndSlice(tCount, out Inventory[] tInventory);

            Inventory = tInventory;

            return next;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(Count)
                .WriteAndSlice(Inventory);
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
            var invc = data.ReadFromBuffer<VarInt>(ref roffset);

            Inventory = new Inventory[invc];
            for (var x = 0; x < invc; x++)
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
#endif
    }

}