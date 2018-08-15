using hashstream.bitcoin_lib;
using hashstream.bitcoin_lib.P2P;
using System;
using System.Net;
using System.Threading.Tasks;

namespace hashstream.bitcoin_node_lib
{
    public class BitcoinNodePeer
    {
        private Guid Id { get; set; }
        private BitcoinPeer Peer { get; set; }

        public async Task WriteMessage<T>(T msg) where T : IStreamable, ICommand => await Peer.WriteMessage(msg);

        public BitcoinNodePeer(BitcoinPeer p, Guid id)
        {
            Id = id;

            Peer = p;
            Peer.OnAddr += Peer_OnAddr;
            Peer.OnAlert += Peer_OnAlert;
            Peer.OnFeeFilter += Peer_OnFeeFilter;
            Peer.OnFilterAdd += Peer_OnFilterAdd;
            Peer.OnFilterClear += Peer_OnFilterClear;
            Peer.OnFilterLoad += Peer_OnFilterLoad;
            Peer.OnGetAddr += Peer_OnGetAddr;
            Peer.OnGetBlocks += Peer_OnGetBlocks;
            Peer.OnGetData += Peer_OnGetData;
            Peer.OnGetHeaders += Peer_OnGetHeaders;
            Peer.OnHeaders += Peer_OnHeaders;
            Peer.OnInv += Peer_OnInv;
            Peer.OnMemPool += Peer_OnMemPool;
            Peer.OnNotFound += Peer_OnNotFound;
            Peer.OnPing += Peer_OnPing;
            Peer.OnPong += Peer_OnPong;
            Peer.OnReject += Peer_OnReject;
            Peer.OnSendHeaders += Peer_OnSendHeaders;
            Peer.OnVerAck += Peer_OnVerAck;
            Peer.OnVersion += Peer_OnVersion;

            Peer.Start();
        }

        public async Task SendVersion()
        {
            //Send version
            var v = new bitcoin_lib.P2P.Version(BitcoinNode.UserAgent);
            var nd = new byte[9];
            new Random().NextBytes(nd);

            v.Nonce = BitConverter.ToUInt64(nd, 0);
            v.RecvIp = ((IPEndPoint)Peer.RemoteEndpoint).Address;
            v.RecvPort = (UInt16)((IPEndPoint)Peer.RemoteEndpoint).Port;
            v.RecvServices = 0;
            v.StartHeight = 0;
            v.Timestamp = (UInt64)DateTimeOffset.Now.ToUnixTimeSeconds();
            v.TransIp = IPAddress.None;
            v.TransPort = 0;
            v.TransServices = (UInt64)Services.NODE_NETWORK;
            v.Relay = true;

            await WriteMessage(v);
        }

        public async Task SendAddr()
        {
            var a = new Addr();

            await WriteMessage(a);
        }

        private async Task Peer_OnVersion(BitcoinPeer s, bitcoin_lib.P2P.Version v)
        {
            if (Peer.IsInbound)
            {
                await SendVersion();
            }

            var va = new VerAck();
            await s.WriteMessage(va);

            Console.WriteLine($"Client connected {v.UserAgent}");
        }

        private async Task Peer_OnVerAck(BitcoinPeer s, VerAck va)
        {
            if (Peer.IsInbound)
            {
                await SendAddr();
            }

            await s.WriteMessage(new Ping());
        }

        private async Task Peer_OnSendHeaders(BitcoinPeer s, SendHeaders sh)
        {
            
        }

        private async Task Peer_OnReject(BitcoinPeer s, Reject r)
        {
            
        }

        private async Task Peer_OnPong(BitcoinPeer s, Pong p)
        {
            
        }

        private async Task Peer_OnPing(BitcoinPeer s, Ping p)
        {
            var pong = new Pong();
            pong.Nonce = p.Nonce;

            await s.WriteMessage(pong);
        }

        private async Task Peer_OnNotFound(BitcoinPeer s, NotFound nf)
        {
           
        }

        private async Task Peer_OnMemPool(BitcoinPeer s, MemPool mp)
        {
            
        }

        private async Task Peer_OnInv(BitcoinPeer s, Inv i)
        {
            
        }

        private async Task Peer_OnHeaders(BitcoinPeer s, Headers h)
        {
            
        }

        private async Task Peer_OnGetHeaders(BitcoinPeer s, GetHeaders gh)
        {
            
        }

        private async Task Peer_OnGetData(BitcoinPeer s, GetData gd)
        {
            
        }

        private async Task Peer_OnGetBlocks(BitcoinPeer s, GetBlocks gb)
        {
            
        }

        private async Task Peer_OnGetAddr(BitcoinPeer s, GetAddr a)
        {
            
        }

        private async Task Peer_OnFilterLoad(BitcoinPeer s, FilterLoad f)
        {
            
        }

        private async Task Peer_OnFilterClear(BitcoinPeer s, FilterClear f)
        {
            
        }

        private async Task Peer_OnFilterAdd(BitcoinPeer s, FilterAdd f)
        {
            
        }

        private async Task Peer_OnFeeFilter(BitcoinPeer s, FeeFilter f)
        {
            
        }

        private async Task Peer_OnAlert(BitcoinPeer s, Alert a)
        {
            
        }

        private async Task Peer_OnAddr(BitcoinPeer s, Addr a)
        {
            
        }
    }
}
