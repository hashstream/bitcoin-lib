using hashstream.bitcoin_lib.BlockChain;
using System;
using System.Net;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class Version : IStreamable, ICommand
    {
        public string Command => "version";

        public UInt32 HighestVersion { get; set; } = Consensus.Version;
        public UInt64 Services { get; set; }
        public UInt64 Timestamp { get; set; }
        public UInt64 RecvServices { get; set; }
        public IPAddress RecvIp { get; set; }
        public UInt16 RecvPort { get; set; }
        public UInt64 TransServices { get; set; }
        public IPAddress TransIp { get; set; }
        public UInt16 TransPort { get; set; }
        public UInt64 Nonce { get; set; }
        public VarInt UserAgentLength { get; set; }
        public string UserAgent { get; set; }
        public UInt32 StartHeight { get; set; }
        public bool Relay { get; set; }

        public int Size => 85 + UserAgentLength.Size + UserAgentLength;

        public Version()
        {

        }

        public Version(string ua)
        {
            UserAgent = ua;
            UserAgentLength = ua.Length;
        }

        public int ReadFromPayload(byte[] data, int offset)
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
            UserAgentLength = data.ReadFromBuffer<VarInt>(ref roffset);
            UserAgent = data.ReadASCIIFromBuffer(ref roffset, UserAgentLength);
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
            ret.CopyAndIncr(RecvIp.GetAddressBytes(), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(RecvPort), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(TransServices), ref woffset);
            ret.CopyAndIncr(TransIp.GetAddressBytes(), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(TransPort), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(Nonce), ref woffset);
            ret.CopyAndIncr(UserAgentLength.ToArray(), ref woffset);
            ret.CopyAndIncr(System.Text.Encoding.ASCII.GetBytes(UserAgent), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(StartHeight), ref woffset);

            ret[woffset] = Relay ? (byte)0x01 : (byte)0x00;

            return ret;
        }
    }
}