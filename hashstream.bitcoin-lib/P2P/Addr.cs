using hashstream.bitcoin_lib.BlockChain;
using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class Addr : IStreamable, ICommand
    {
        public VarInt IpCount { get; set; }
        public IP[] Ips { get; set; }

        public string Command => "addr";

        public void ReadFromPayload(byte[] data, int offset)
        {
            IpCount = new VarInt(0);
            IpCount.ReadFromPayload(data, offset);

            var ipOffset = offset + IpCount.Size;
            Ips = new IP[IpCount];
            for (var x = 0; x < IpCount; x++)
            {
                var ip = new IP();
                ip.ReadFromPayload(data, ipOffset);
                Ips[x] = ip;
                ipOffset += ip.Size;
            }
        }

        public byte[] ToArray()
        {
            //ew
            var ix = IpCount * 30;
            var ret = new byte[ix + IpCount.Size];

            var ic = IpCount.ToArray();
            Array.Copy(ic, 0, ret, 0, ic.Length);

            var ipOffset = ic.Length;
            for (var x = 0; x < IpCount; x++)
            {
                var ip = Ips[x].ToArray();
                Array.Copy(ret, 0, ret, ipOffset, ip.Length);
                ipOffset += ip.Length;
            }

            return ret;
        }
    }
}
