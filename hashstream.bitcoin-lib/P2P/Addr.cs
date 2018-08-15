using hashstream.bitcoin_lib.BlockChain;
using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class Addr : IStreamable, ICommand
    {
        public VarInt IpCount => Ips?.Length;
        public IP[] Ips { get; set; } = new IP[0];

        public string Command => "addr";

        public int Size => IpCount.Size + (IP.StaticSize * IpCount);

#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var ret = data.ReadAndSlice(out VarInt tIpc)
                .ReadAndSlice(tIpc, out IP[] tIps);
            
            Ips = tIps;

            return ret;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(IpCount)
                .WriteAndSlice(Ips);
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];
            WriteToPayload(ret);
            return ret;
        }
#else
        public int ReadFromPayload(byte[] data, int offset)
        {
            var roffset = offset;
            var ipc = data.ReadFromBuffer<VarInt>(ref roffset);

            Ips = new IP[ipc];
            for (var x = 0; x < ipc; x++)
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
#endif
    }
}
