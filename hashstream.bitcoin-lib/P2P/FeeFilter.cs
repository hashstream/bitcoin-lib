using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class FeeFilter : IStreamable, ICommand
    {
        public UInt64 FeeRate { get; set; }

        public string Command => "feefilter";

        public void ReadFromPayload(byte[] data, int offset)
        {
            FeeRate = BitConverter.ToUInt64(data, offset);
        }

        public byte[] ToArray()
        {
            return BitConverter.GetBytes(FeeRate);
        }
    }
}
