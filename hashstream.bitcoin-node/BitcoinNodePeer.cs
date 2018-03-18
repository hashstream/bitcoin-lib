using hashstream.bitcoin_lib.P2P;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace hashstream.bitcoin_node_lib
{
    public class BitcoinNodePeer
    {
        private BitcoinPeer Peer { get; set; }
        private bitcoin_lib.P2P.Version Ver { get; set; }

        public BitcoinNodePeer(BitcoinPeer p)
        {
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

            //Send version
            Ver = new bitcoin_lib.P2P.Version($"/hashstream:0.0.1-alpha/");
            var nd = new byte[9];
            new Random().NextBytes(nd);
            Ver.Nonce = BitConverter.ToUInt64(nd, 0);
            Ver.RecvIp = ((IPEndPoint)Peer.RemoteEndpoint).Address;
            Ver.RecvPort = (UInt16)((IPEndPoint)Peer.RemoteEndpoint).Port;
            Ver.RecvServices = 0;
            Ver.StartHeight = 0;
            Ver.Timestamp = (UInt64)DateTimeOffset.Now.ToUnixTimeSeconds();
            Ver.TransIp = IPAddress.None;
            Ver.TransPort = 0;
            Ver.TransServices = (UInt64)Services.NODE_NETWORK;

            Peer.WriteMessage(Ver);
        }

        private async Task Peer_OnVersion(BitcoinPeer s, bitcoin_lib.P2P.Version v)
        {
            var va = new VerAck();
            await s.WriteMessage(va);

            Console.WriteLine($"Client connected {v.UserAgent}");
        }

        private Task Peer_OnVerAck(BitcoinPeer s, VerAck va)
        {
            throw new NotImplementedException();
        }

        private Task Peer_OnSendHeaders(BitcoinPeer s, SendHeaders sh)
        {
            throw new NotImplementedException();
        }

        private Task Peer_OnReject(BitcoinPeer s, Reject r)
        {
            throw new NotImplementedException();
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

        private Task Peer_OnNotFound(BitcoinPeer s, NotFound nf)
        {
            throw new NotImplementedException();
        }

        private Task Peer_OnMemPool(BitcoinPeer s, MemPool mp)
        {
            throw new NotImplementedException();
        }

        private Task Peer_OnInv(BitcoinPeer s, Inv i)
        {
            throw new NotImplementedException();
        }

        private Task Peer_OnHeaders(BitcoinPeer s, Headers h)
        {
            throw new NotImplementedException();
        }

        private Task Peer_OnGetHeaders(BitcoinPeer s, GetHeaders gh)
        {
            throw new NotImplementedException();
        }

        private Task Peer_OnGetData(BitcoinPeer s, GetData gd)
        {
            throw new NotImplementedException();
        }

        private Task Peer_OnGetBlocks(BitcoinPeer s, GetBlocks gb)
        {
            throw new NotImplementedException();
        }

        private Task Peer_OnGetAddr(BitcoinPeer s, GetAddr a)
        {
            throw new NotImplementedException();
        }

        private Task Peer_OnFilterLoad(BitcoinPeer s, FilterLoad f)
        {
            throw new NotImplementedException();
        }

        private Task Peer_OnFilterClear(BitcoinPeer s, FilterClear f)
        {
            throw new NotImplementedException();
        }

        private Task Peer_OnFilterAdd(BitcoinPeer s, FilterAdd f)
        {
            throw new NotImplementedException();
        }

        private Task Peer_OnFeeFilter(BitcoinPeer s, FeeFilter f)
        {
            throw new NotImplementedException();
        }

        private Task Peer_OnAlert(BitcoinPeer s, Alert a)
        {
            throw new NotImplementedException();
        }

        private Task Peer_OnAddr(BitcoinPeer s, Addr a)
        {
            throw new NotImplementedException();
        }
    }
}
