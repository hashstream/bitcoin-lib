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

        public string Command => null;

        public void ReadFromPayload(byte[] data, int offset)
        {
            Type = (InventoryType)BitConverter.ToUInt32(data, offset);

            Hash = new Hash();
            Hash.ReadFromPayload(data, offset + 4);
        }

        public byte[] ToArray()
        {
            var t = BitConverter.GetBytes((UInt32)Type);
            var ret = new byte[Hash.HashBytes.Length + t.Length];
            Buffer.BlockCopy(t, 0, ret, 0, t.Length);
            Buffer.BlockCopy(Hash.HashBytes, 0, ret, t.Length, Hash.HashBytes.Length);

            return ret;
        }
    }
}
