using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class SendCMPCT : IStreamable, ICommand
    {
        public bool Enabled { get; set; }
        public UInt64 Version { get; set; }

        public string Command => "sendcmpct";

        public int Size => 9;

#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var ret = data.ReadAndSlice(out byte tEnabled)
                .ReadAndSlice(out UInt64 tVersion);

            Enabled = (tEnabled & 0x01) == 0x01;
            Version = tVersion;

            return ret;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(Enabled ? 0x01 : 0x00)
                .WriteAndSlice(Version);
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
            var tEnabled = data[roffset]; roffset++;
            var tVersion = data.ReadUInt64FromBuffer(ref roffset);

            Enabled = (tEnabled & 0x01) == 1;
            Version = tVersion;

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];

            ret.CopyAndIncr(new byte[] { (byte)(Enabled ? 0x01 : 00) }, ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(Version), woffset);

            return ret;
        }
#endif
    }
}
