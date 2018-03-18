using hashstream.bitcoin_lib.P2P;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace hashstream.bitcoin_node_lib
{
    public class BitcoinNode
    {
        private Socket Sock { get; set; }
        private Task AcceptTask { get; set; }
        private Task CheckPeerListTask { get; set; }
        private CancellationTokenSource Cts { get; set; }
        private List<string> DNSSeeds { get; set; } = new List<string>()
        {
            "seed.bitcoin.sipa.be",
            "dnsseed.bluematt.me",
            "dnsseed.bitcoin.dashjr.org",
            "seed.bitcoinstats.com",
            "seed.bitcoin.jonasschnelli.ch",
            "seed.btc.petertodd.org"
        };

        public delegate void Log(string msg);
        public delegate void PeerConnected(BitcoinNodePeer np);
        public delegate void PeerDisconnected(BitcoinNodePeer np);

        public event Log OnLog;
        public event PeerConnected OnPeerConnected;
        public event PeerDisconnected OnPeerDisconnected;

        public BitcoinNode(IPEndPoint ip = null)
        {
            if(ip == null)
            {
                ip = new IPEndPoint(IPAddress.Any, 8333);
            }

            Cts = new CancellationTokenSource();
            Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Sock.Bind(ip);
        }

        public void Start()
        {
            Sock.Listen(1024);
            AcceptTask = Accept();
            CheckPeerListTask = CheckPeerList();

            OnLog?.Invoke($"Node Started on {Sock.LocalEndPoint}!");
        }

        private async Task Accept()
        {
            while (!Cts.IsCancellationRequested)
            {
                var ns = await Sock.AcceptAsync();
                if (ns != null)
                {
                    new BitcoinNodePeer(new BitcoinPeer(ns));
                }
            }
        }

        private async Task CheckPeerList()
        {
            while (!Cts.IsCancellationRequested)
            {

                await Task.Delay(1000);
            }
        }

        public void AddPeer(IPEndPoint ip)
        {
            new BitcoinNodePeer(new BitcoinPeer(ip));
        }
    }
}
