using hashstream.bitcoin_lib;
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

        private ConcurrentDictionary<Guid, T> Peers { get; set; } = new ConcurrentDictionary<Guid, T>();

        public delegate void Log(string msg);
        public delegate void PeerConnected(T np);
        public delegate void PeerDisconnected(T np);

        public event Log OnLog;

        public ChainParams ChainParams { get; private set; }

        public BitcoinNode(ChainParams cp, IPEndPoint ip = null)
        {
            ChainParams = cp;
            if (ip == null)
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
                    np.OnStop += Node_OnStop;
                    np.Init(this, new BitcoinPeer(ChainParams, ns, true), id);

                    if(Peers.TryAdd(id, np))
                    {
                        OnLog?.Invoke($"New peer added: {np.RemoteEndpoint}!");
                    }
                    
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

        /// <summary>
        /// Adds a new peer to the node, all exceptions are thrown back from connection errors.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="cp"></param>
        /// <returns></returns>
        /// <exception cref="SocketException"></exception>
        public async Task AddPeer(IPEndPoint ip, ChainParams cp = null)
        {
            var id = Guid.NewGuid();

            var np = new T();
            np.OnStop += Node_OnStop;
            var nc = new BitcoinPeer(cp ?? ChainParams);
            await nc.ConnectAsync(ip);

            np.Init(this, nc, id);
            await np.SendVersion();

            if(Peers.TryAdd(id, np))
            {
                OnLog?.Invoke($"New peer added: {np.RemoteEndpoint}!");
            }
        }

        private void Node_OnStop(Guid g)
        {
            if(Peers.TryRemove(g, out T peer))
            {
                OnLog?.Invoke($"Peer disconnected: {peer.RemoteEndpoint}");
            }
        }

        public IEnumerable<T> EnumeratePeers()
        {
            foreach (var node in Peers)
            {
                yield return node.Value;
            }
        }
    }
}
