using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class FeeFilter : IStreamable, ICommand
    {
        public UInt64 FeeRate { get; set; }

        public string Command => "feefilter";

        public int Size => StaticSize;

        public static int StaticSize => 8;
        
#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out UInt64 tFeeRate);

            FeeRate = tFeeRate;

            return next;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(FeeRate);
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
            FeeRate = data.ReadUInt64FromBuffer(ref roffset);

            return Size;
        }

        public byte[] ToArray()
        {
            return BitConverter.GetBytes(FeeRate);
        }
#endif
    }
}
