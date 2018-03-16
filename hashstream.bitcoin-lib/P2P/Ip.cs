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
            Buffer.BlockCopy(data, offset + 12, ip, 0, ip.Length);
            Ip = new IPAddress(ip);

            Port = BitConverter.ToUInt16(data, offset + 28);
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];

            var t = BitConverter.GetBytes(Time);
            Buffer.BlockCopy(t, 0, ret, 0, t.Length);

            var s = BitConverter.GetBytes(Services);
            Buffer.BlockCopy(s, 0, ret, 4, s.Length);

            var i = Ip.MapToIPv6().GetAddressBytes();
            Buffer.BlockCopy(i, 0, ret, 12, i.Length);

            var p = BitConverter.GetBytes(Port);
            Buffer.BlockCopy(p, 0, ret, 28, p.Length);

            return ret;
        }
    }
}
