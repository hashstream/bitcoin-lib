using System;
using System.Net;

namespace hashstream.bitcoin_lib.P2P
{
    public class IP : IStreamable
    {
        public UInt32 Time { get; set; }
        public UInt64 Services { get; set; }
        public IPAddress Ip { get; set; }
        public UInt16 Port { get; set; }

        public static int Size => 30;

        public int ReadFromPayload(byte[] data, int offset)
        {
            var roffset = offset;
            Time = data.ReadUInt32FromBuffer(ref roffset);
            Services = data.ReadUInt64FromBuffer(ref roffset);

            var ip = new byte[16];
            Array.Copy(data, roffset, ip, 0, ip.Length);
            Ip = new IPAddress(ip);
            roffset += ip.Length;
            
            Port = data.ReadUInt16FromBuffer(ref roffset);

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];

            ret.CopyAndIncr(BitConverter.GetBytes(Time), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(Services), ref woffset);
            ret.CopyAndIncr(Ip.MapToIPv6().GetAddressBytes(), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(Port), woffset);


            return ret;
        }
    }
}
