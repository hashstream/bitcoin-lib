using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class FeeFilter : IStreamable, ICommand
    {
        public UInt64 FeeRate { get; set; }

        public string Command => "feefilter";

        public static int Size => 8;

        public int ReadFromPayload(byte[] data, int offset)
        {
            var roffset = offset;
            FeeRate = data.ReadUInt64FromBuffer(ref roffset);

            return Size;
        }

        public byte[] ToArray()
        {
            return BitConverter.GetBytes(FeeRate);
        }
    }
}
