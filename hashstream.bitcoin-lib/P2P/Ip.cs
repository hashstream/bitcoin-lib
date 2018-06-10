using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class IP : IStreamable
    {
        public UInt32 Time { get; set; }
        public UInt64 Services { get; set; }
        public IPAddress Ip { get; set; }
        public UInt16 Port { get; set; }

        public int Size => 30;

        public void ReadFromPayload(byte[] data, int offset)
        {
            Time = BitConverter.ToUInt32(data, offset);
            Services = BitConverter.ToUInt64(data, offset + 4);

            var ip = new byte[16];
            Array.Copy(data, offset + 12, ip, 0, ip.Length);
            Ip = new IPAddress(ip);

            var pt = new byte[2];
            Array.Copy(data, offset + 28, pt, 0, pt.Length);
            Array.Reverse(pt);
            Port = BitConverter.ToUInt16(pt, 0);
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];

            var t = BitConverter.GetBytes(Time);
            Array.Copy(t, 0, ret, 0, t.Length);

            var s = BitConverter.GetBytes(Services);
            Array.Copy(s, 0, ret, 4, s.Length);

            var i = Ip.MapToIPv6().GetAddressBytes();
            Array.Copy(i, 0, ret, 12, i.Length);

            var p = BitConverter.GetBytes(Port);
            Array.Copy(p, 0, ret, 28, p.Length);

            return ret;
        }
    }
}
