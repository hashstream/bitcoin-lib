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

        public void ReadFromPayload(byte[] data, int offset)
        {
            FilterBytes = new VarInt(0);
            FilterBytes.ReadFromPayload(data, offset);

            Filter = new byte[FilterBytes];
            Array.Copy(data, offset + FilterBytes.Size, Filter, 0, Filter.Length);

            HashFunctions = BitConverter.ToUInt32(data, offset + FilterBytes + FilterBytes.Size);
            Tweak = BitConverter.ToUInt32(data, offset + FilterBytes + FilterBytes.Size + 4);
            Flags = data[offset + FilterBytes + FilterBytes.Size + 8];
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];
            var fb = FilterBytes.ToArray();
            Array.Copy(fb, 0, ret, 0, fb.Length);
            Array.Copy(Filter, 0, ret, fb.Length, Filter.Length);

            var hf = BitConverter.GetBytes(HashFunctions);
            Array.Copy(hf, 0, ret, fb.Length + Filter.Length, hf.Length);

            var tw = BitConverter.GetBytes(Tweak);
            Array.Copy(tw, 0, ret, fb.Length + Filter.Length + hf.Length, tw.Length);

            ret[ret.Length - 1] = Flags;

            return ret;
        }
    }
}
