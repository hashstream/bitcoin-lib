using System;
using System.Net;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class Version : IStreamable
    {
        public string Command => "version";

        public UInt32 HighestVersion { get; set; }
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


        public Version(string ua)
        {
            UserAgent = ua;
            UserAgentLength = ua.Length;
        }

        public void ReadFromPayload(byte[] data, int offset)
        {
            HighestVersion = BitConverter.ToUInt32(data, offset);
            Services = BitConverter.ToUInt64(data, offset + 4);
            Timestamp = BitConverter.ToUInt64(data, offset + 12);

            RecvServices = BitConverter.ToUInt64(data, offset + 20);
            var rip = new byte[16];
            Buffer.BlockCopy(data, offset + 28, rip, 0, 16);
            RecvIp = new IPAddress(rip);
            RecvPort = BitConverter.ToUInt16(data, offset + 44);

            TransServices = BitConverter.ToUInt64(data, offset + 46);
            var tip = new byte[16];
            Buffer.BlockCopy(data, offset + 54, tip, 0, 16);
            TransIp = new IPAddress(tip);
            TransPort = BitConverter.ToUInt16(data, offset + 70);

            Nonce = BitConverter.ToUInt64(data, offset + 72);
            UserAgentLength = new VarInt(0);
            UserAgentLength.ReadFromPayload(data, offset + 80);

            var noffset = offset + 80 + UserAgentLength.Size;
            UserAgent = Encoding.ASCII.GetString(data, noffset, (int)UserAgentLength);
            StartHeight = BitConverter.ToUInt32(data, noffset + (int)UserAgentLength);
            Relay = BitConverter.ToBoolean(data, noffset + (int)UserAgentLength + 4);
        }

        public byte[] ToArray()
        {
            var pl = new byte[85 + UserAgentLength.Size + (int)UserAgentLength];

            var hv = BitConverter.GetBytes(HighestVersion);
            Buffer.BlockCopy(hv, 0, pl, 0, hv.Length);

            var sv = BitConverter.GetBytes(Services);
            Buffer.BlockCopy(sv, 0, pl, 4, sv.Length);

            var ts = BitConverter.GetBytes(Timestamp);
            Buffer.BlockCopy(ts, 0, pl, 12, ts.Length);

            var rs = BitConverter.GetBytes(RecvServices);
            Buffer.BlockCopy(rs, 0, pl, 20, rs.Length);

            var ri = RecvIp.GetAddressBytes();
            Buffer.BlockCopy(ri, 0, pl, 28, ri.Length);

            var rp = BitConverter.GetBytes(RecvPort);
            Array.Reverse(rp);
            Buffer.BlockCopy(rp, 0, pl, 44, rp.Length);

            var tss = BitConverter.GetBytes(TransServices);
            Buffer.BlockCopy(tss, 0, pl, 46, tss.Length);

            var ti = TransIp.GetAddressBytes();
            Buffer.BlockCopy(ti, 0, pl, 54, ti.Length);

            var tp = BitConverter.GetBytes(TransPort);
            Array.Reverse(tp);
            Buffer.BlockCopy(tp, 0, pl, 70, tp.Length);

            var nn = BitConverter.GetBytes(Nonce);
            Buffer.BlockCopy(nn, 0, pl, 72, nn.Length);

            var ul = UserAgentLength.ToArray();
            Buffer.BlockCopy(ul, 0, pl, 80, ul.Length);

            var ua = Encoding.ASCII.GetBytes(UserAgent);
            Buffer.BlockCopy(ua, 0, pl, 80 + UserAgentLength.Size, ua.Length);

            var sh = BitConverter.GetBytes(StartHeight);
            Buffer.BlockCopy(sh, 0, pl, 80 + UserAgentLength.Size + (int)UserAgentLength, sh.Length);

            pl[pl.Length-1] = Relay ? (byte)0x01 : (byte)0x00;

            return pl;
        }
    }
}
