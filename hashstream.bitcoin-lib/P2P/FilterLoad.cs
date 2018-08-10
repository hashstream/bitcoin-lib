using hashstream.bitcoin_lib.BlockChain;
using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class FilterLoad : IStreamable, ICommand
    {
        public VarInt FilterBytes { get; set; }
        public byte[] Filter { get; set; }
        public UInt32 HashFunctions { get; set; }
        public UInt32 Tweak { get; set; }
        public byte Flags { get; set; }

        public string Command => "filterload";

        public int Size => 9 + FilterBytes + FilterBytes.Size;

        public int ReadFromPayload(byte[] data, int offset)
        {
            var roffset = offset;
            FilterBytes = data.ReadFromBuffer<VarInt>(ref roffset);

            Filter = new byte[FilterBytes];
            Array.Copy(data, roffset + FilterBytes.Size, Filter, 0, Filter.Length);
            roffset += FilterBytes.Size + FilterBytes;

            HashFunctions = data.ReadUInt32FromBuffer(ref roffset);
            Tweak = data.ReadUInt32FromBuffer(ref roffset);
            Flags = data[roffset];

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];

            ret.CopyAndIncr(FilterBytes.ToArray(), ref woffset);
            ret.CopyAndIncr(Filter, ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(HashFunctions), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(Tweak), woffset);

            ret[ret.Length - 1] = Flags;

            return ret;
        }
    }
}
