using hashstream.bitcoin_lib;
using hashstream.bitcoin_lib.BlockChain;
using hashstream.bitcoin_lib.P2P;
using System;
using System.Net;

namespace hashstream.bitcoin_node
{
    public class PeerInfo : IStreamable, IHash
    {
        public IPEndPoint Ip { get; set; }
        public DateTime LastSeen { get; set; }
        public bitcoin_lib.P2P.Version LastVersion { get; set; }

        public int Size => 26 + LastVersion.Size;

        public Hash Hash => Ip.AsHash();

        public IP ToIP()
        {
            var ret = new IP();
            ret.Ip = Ip.Address;
            ret.Port = (ushort)Ip.Port;
            ret.Services = LastVersion.Services;
            ret.Time = (UInt32)(LastSeen - new DateTime(1970, 1, 1)).TotalSeconds;

            return ret;
        }

#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out IPAddress oIp)
                .ReadAndSlice(out UInt16 oPort)
                .ReadAndSlice(out Int64 oLastSeen)
                .ReadAndSlice(out bitcoin_lib.P2P.Version oVersion);

            Ip = new IPEndPoint(oIp, oPort);
            LastSeen = new DateTime(oLastSeen);
            LastVersion = oVersion;

            return next;
        }
        
        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(Ip.Address)
                .WriteAndSlice((UInt16)Ip.Port)
                .WriteAndSlice(LastSeen.Ticks)
                .WriteAndSlice(LastVersion);
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
            throw new NotImplementedException();
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];
            var roffset = 0;

            ret.CopyAndIncr(BitConverter.GetBytes(LastSeen.Ticks), ref roffset);
            ret.CopyAndIncr(LastVersion.ToArray(), ref roffset);

            return ret;
        }
#endif
    }
}
