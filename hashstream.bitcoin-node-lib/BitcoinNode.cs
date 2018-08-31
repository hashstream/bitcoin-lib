using hashstream.bitcoin_lib.P2P;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace hashstream.bitcoin_node_lib
{
    public class BitcoinNode<T> where T : PeerHandler, new()
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

        private ConcurrentDictionary<Guid, T> Peers { get; set; } = new ConcurrentDictionary<Guid, T>();

        public delegate void Log(string msg);
        public delegate void PeerConnected(T np);
        public delegate void PeerDisconnected(T np);

        public event Log OnLog;
        public event PeerConnected OnPeerConnected;
        public event PeerDisconnected OnPeerDisconnected;

        public static string UserAgent { get; set; } = "/hashstream-node:0.1/";

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
#if NET461
            var sem_net461 = new SemaphoreSlim(1);
#endif
            while (!Cts.IsCancellationRequested)
            {
#if NET461
                Socket ns = null;
                var saa = new SocketAsyncEventArgs();
                saa.Completed += (s, e) => 
                {
                    ns = e.AcceptSocket;
                    sem_net461.Release();
                };

                if (!Sock.AcceptAsync(saa))
                {
                    ns = saa.AcceptSocket;
                }
                else
                {
                    await sem_net461.WaitAsync();
                }
#else
                var ns = await Sock.AcceptAsync();
#endif
                if (ns != null)
                {
                    var id = Guid.NewGuid();
                    var np = new T();
                    np.Init(new BitcoinPeer(ns, true), id);

                    Peers.TryAdd(id, np);
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

        public async Task AddPeer(IPEndPoint ip)
        {
            var id = Guid.NewGuid();
            var np = new T();
            np.Init(new BitcoinPeer(ip), id);
            await np.SendVersion();

            Peers.TryAdd(id, np);
        }

        public IEnumerable<T> EnumeratePeers()
        {
            foreach(var node in Peers)
            {
                yield return node.Value;
            }
        }
    }
}
