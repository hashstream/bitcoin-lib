using hashstream.bitcoin_lib.BlockChain;
using System;

namespace hashstream.bitcoin_lib.P2P
{
    public class Inventory : IStreamable
    {
        public InventoryType Type { get; set; }
        public Hash Hash { get; set; }

        public int Size => StaticSize;

        public static int StaticSize => 4 + Hash.StaticSize;

#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out UInt32 tType)
                .ReadAndSlice(out Hash tHash);

            Type = (InventoryType)tType;
            Hash = tHash;
            return next;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice((UInt32)Type)
                .WriteAndSlice(Hash);
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
#endif
    }
    public enum InventoryType
    {
        MSG_TX = 1,
        MSG_BLOCK = 2,
        MSG_FILTERED_BLOCK = 3
    }

}
