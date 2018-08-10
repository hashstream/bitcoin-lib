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

        public int Size => IpCount.Size + (IP.Size * IpCount);

        public int ReadFromPayload(byte[] data, int offset)
        {
            var roffset = offset;
            IpCount = data.ReadFromBuffer<VarInt>(ref roffset);

            Ips = new IP[IpCount];
            for (var x = 0; x < IpCount; x++)
            {
                Ips[x] = data.ReadFromBuffer<IP>(ref roffset);
            }

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[IpCount * 30 + IpCount.Size];

            ret.CopyAndIncr(IpCount.ToArray(), ref woffset);
            
            foreach(var ip in Ips)
            {
                ret.CopyAndIncr(ip.ToArray(), ref woffset);
            }

            return ret;
        }
    }
}
