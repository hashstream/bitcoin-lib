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
            Array.Copy(data, offset + 28, rip, 0, 16);
            RecvIp = new IPAddress(rip);
            RecvPort = BitConverter.ToUInt16(data, offset + 44);

            TransServices = BitConverter.ToUInt64(data, offset + 46);
            var tip = new byte[16];
            Array.Copy(data, offset + 54, tip, 0, 16);
            TransIp = new IPAddress(tip);
            TransPort = BitConverter.ToUInt16(data, offset + 70);

            Nonce = BitConverter.ToUInt64(data, offset + 72);
            UserAgentLength = new VarInt(0);
            UserAgentLength.ReadFromPayload(data, offset + 80);

            var noffset = offset + 80 + UserAgentLength.Size;
            UserAgent = System.Text.Encoding.ASCII.GetString(data, noffset, UserAgentLength);
            StartHeight = BitConverter.ToUInt32(data, noffset + UserAgentLength);
            Relay = BitConverter.ToBoolean(data, noffset + UserAgentLength + 4);
        }

        public byte[] ToArray()
        {
            var pl = new byte[85 + UserAgentLength.Size + UserAgentLength];

            var hv = BitConverter.GetBytes(HighestVersion);
            Array.Copy(hv, 0, pl, 0, hv.Length);

            var sv = BitConverter.GetBytes(Services);
            Array.Copy(sv, 0, pl, 4, sv.Length);

            var ts = BitConverter.GetBytes(Timestamp);
            Array.Copy(ts, 0, pl, 12, ts.Length);

            var rs = BitConverter.GetBytes(RecvServices);
            Array.Copy(rs, 0, pl, 20, rs.Length);

            var ri = RecvIp.GetAddressBytes();
            Array.Copy(ri, 0, pl, 28, ri.Length);

            var rp = BitConverter.GetBytes(RecvPort);
            Array.Reverse(rp);
            Array.Copy(rp, 0, pl, 44, rp.Length);

            var tss = BitConverter.GetBytes(TransServices);
            Array.Copy(tss, 0, pl, 46, tss.Length);

            var ti = TransIp.GetAddressBytes();
            Array.Copy(ti, 0, pl, 54, ti.Length);

            var tp = BitConverter.GetBytes(TransPort);
            Array.Reverse(tp);
            Array.Copy(tp, 0, pl, 70, tp.Length);

            var nn = BitConverter.GetBytes(Nonce);
            Array.Copy(nn, 0, pl, 72, nn.Length);

            var ul = UserAgentLength.ToArray();
            Array.Copy(ul, 0, pl, 80, ul.Length);

            var ua = System.Text.Encoding.ASCII.GetBytes(UserAgent);
            Array.Copy(ua, 0, pl, 80 + UserAgentLength.Size, ua.Length);

            var sh = BitConverter.GetBytes(StartHeight);
            Array.Copy(sh, 0, pl, 80 + UserAgentLength.Size + UserAgentLength, sh.Length);

            pl[pl.Length-1] = Relay ? (byte)0x01 : (byte)0x00;

            return pl;
        }
    }
}