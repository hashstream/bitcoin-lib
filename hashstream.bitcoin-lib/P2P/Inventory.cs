using hashstream.bitcoin_lib.BlockChain;
using System;

namespace hashstream.bitcoin_lib.P2P
{
    public enum InventoryType
    {
        MSG_TX = 1,
        MSG_BLOCK = 2,
        MSG_FILTERED_BLOCK = 3
    }

    public class Inventory : IStreamable
    {
        public InventoryType Type { get; set; }
        public Hash Hash { get; set; }

        public static int Size => 4 + Hash.Size;

        public int ReadFromPayload(byte[] data, int offset)
        {
            var roffset = offset;

            Type = (InventoryType)data.ReadUInt32FromBuffer(ref roffset);
            Hash = data.ReadFromBuffer<Hash>(ref roffset);

            return Size;
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];

            ret.CopyAndIncr(BitConverter.GetBytes((UInt32)Type), 0);
            ret.CopyAndIncr(Hash.ToArray(), 4);

            return ret;
        }
    }
}
