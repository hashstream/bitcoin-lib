using hashstream.bitcoin_lib.BlockChain;
using System;
using System.Net;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class Version : IStreamable, ICommand
    {
        public string Command => "version";

        public UInt32 HighestVersion { get; set; }
        public UInt64 Services { get; set; }
        public UInt64 Timestamp { get; set; }
        public UInt64 RecvServices { get; set; }
        public IPAddress RecvIp { get; set; } = IPAddress.None;
        public UInt16 RecvPort { get; set; }
        public UInt64 TransServices { get; set; }
        public IPAddress TransIp { get; set; } = IPAddress.None;
        public UInt16 TransPort { get; set; }
        public UInt64 Nonce { get; set; }
        public VarInt UserAgentLength => UserAgent?.Length;
        public string UserAgent { get; set; }
        public UInt32 StartHeight { get; set; }
        public bool Relay { get; set; }

        public int Size => 85 + UserAgentLength.Size + UserAgentLength;

        public Version()
        {
            UserAgent = "";
        }

        public Version(string ua)
        {
            UserAgent = ua;
        }

#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out UInt32 tHighestVersion)
                .ReadAndSlice(out UInt64 tServices)
                .ReadAndSlice(out UInt64 tTimestamp)
                .ReadAndSlice(out UInt64 tRecvServices)
                .ReadAndSlice(out IPAddress tRecvIp)
                .ReadAndSlice(out UInt16 tRecvPort)
                .ReadAndSlice(out UInt64 tTransServices)
                .ReadAndSlice(out IPAddress tTransIp)
                .ReadAndSlice(out UInt16 tTransPort)
                .ReadAndSlice(out UInt64 tNonce)
                .ReadAndSlice(out VarInt tUserAgentLength)
                .ReadAndSlice(tUserAgentLength, out string tUserAgent)
                .ReadAndSlice(out UInt32 tStartHeight)
                .ReadAndSlice(out byte tRelay);

            HighestVersion = tHighestVersion;
            Services = tServices;
            Timestamp = tTimestamp;
            RecvServices = tRecvServices;
            RecvIp = tRecvIp;
            RecvPort = tRecvPort;
            TransServices = tTransServices;
            TransIp = tTransIp;
            TransPort = tTransPort;
            Nonce = tNonce;
            UserAgent = tUserAgent;
            StartHeight = tStartHeight;
            Relay = (tRelay & 0x01) != 0;

            return next;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(HighestVersion)
                .WriteAndSlice(Services)
                .WriteAndSlice(Timestamp)
                .WriteAndSlice(RecvServices)
                .WriteAndSlice(RecvIp)
                .WriteAndSlice(RecvPort)
                .WriteAndSlice(TransServices)
                .WriteAndSlice(TransIp)
                .WriteAndSlice(TransPort)
                .WriteAndSlice(Nonce)
                .WriteAndSlice(UserAgentLength)
                .WriteAndSlice(UserAgent)
                .WriteAndSlice(StartHeight)
                .WriteAndSlice(Relay ? (byte)0x01 : (byte)0x00);
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

            HighestVersion = data.ReadUInt32FromBuffer(ref roffset);
            Services = data.ReadUInt64FromBuffer(ref roffset);
            Timestamp = data.ReadUInt64FromBuffer(ref roffset);
            RecvServices = data.ReadUInt64FromBuffer(ref roffset);
            RecvIp = data.ReadIPAddressFromBuffer(ref roffset);
            RecvPort = data.ReadUInt16FromBuffer(ref roffset);
            TransServices = data.ReadUInt64FromBuffer(ref roffset);
            TransIp = data.ReadIPAddressFromBuffer(ref roffset);
            TransPort = data.ReadUInt16FromBuffer(ref roffset);
            Nonce = data.ReadUInt64FromBuffer(ref roffset);
            var ual = data.ReadFromBuffer<VarInt>(ref roffset);
            UserAgent = data.ReadASCIIFromBuffer(ref roffset, ual);
            StartHeight = data.ReadUInt32FromBuffer(ref roffset);
            Relay = BitConverter.ToBoolean(data, roffset);

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];
            
            ret.CopyAndIncr(BitConverter.GetBytes(HighestVersion), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(Services), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(Timestamp), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(RecvServices), ref woffset);
            ret.CopyAndIncr(RecvIp.MapToIPv6().GetAddressBytes(), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(RecvPort), ref woffset, true);
            ret.CopyAndIncr(BitConverter.GetBytes(TransServices), ref woffset, true);
            ret.CopyAndIncr(TransIp.MapToIPv6().GetAddressBytes(), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(TransPort), ref woffset, true);
            ret.CopyAndIncr(BitConverter.GetBytes(Nonce), ref woffset);
            ret.CopyAndIncr(UserAgentLength.ToArray(), ref woffset);
            ret.CopyAndIncr(System.Text.Encoding.ASCII.GetBytes(UserAgent), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(StartHeight), ref woffset);

            ret[woffset] = Relay ? (byte)0x01 : (byte)0x00;

            return ret;
        }
#endif
    }

    //https://github.com/bitcoin/bitcoin/blob/bdbd654df8ddf5945256f2ccf85feae620f6f0d5/src/protocol.h#L247
    [Flags]
    public enum Services
    {
        NODE_NONE = 0,
        NODE_NETWORK = (1 << 0),
        NODE_GETUTXO = (1 << 1),
        NODE_BLOOM = (1 << 2),
        NODE_WITNESS = (1 << 3),
        NODE_XTHIN = (1 << 4),
        NODE_NETWORK_LIMITED = (1 << 10)
    }
}