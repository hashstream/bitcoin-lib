using hashstream.bitcoin_lib;
using hashstream.bitcoin_lib.BlockChain;
using hashstream.bitcoin_lib.P2P;
using hashstream.bitcoin_node_lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace hashstream.bitcoin_node
{
    public class BitcoinNodePeer : PeerHandler
    {
        public Guid Id { get; private set; }
        public BitcoinNode<BitcoinNodePeer> Node { get; private set; }
        public BitcoinPeer Peer { get; private set; }
        public bitcoin_lib.P2P.Version PeerVersion { get; private set; }

        public ulong CMPCTBlockVersion { get; private set; }

        public event PeerStoppingEvent OnStop;

        public async Task WriteMessage<T>(T msg) where T : IStreamable, ICommand => await Peer.WriteMessage(msg);

        private Stopwatch PingTimer { get; set; } = new Stopwatch();
        public TimeSpan LastPing => PingTimer.Elapsed;

        public IPEndPoint RemoteEndpoint => Peer?.RemoteEndpoint;

        public async Task StartPing()
        {
            var p = new Ping();
            await WriteMessage(p);
            PingTimer.Restart();
        }

        public void Init<T>(BitcoinNode<T> node, BitcoinPeer peer, Guid id) where T : PeerHandler, new()
        {
            Node = node as BitcoinNode<BitcoinNodePeer>;
            Id = id;

            Peer = peer;
            Peer.OnMessage += Peer_OnMessage;
            Peer.OnStopping += Peer_OnStopping;
            Peer.Start();
        }

        private void Peer_OnStopping()
        {
            OnStop?.Invoke(Id);
        }

        public void Disconnect()
        {
            Peer.Stop();
        }

        public async Task SendVersion()
        {
            //Send version
            var v = new bitcoin_lib.P2P.Version(Peer.ChainParams.UserAgent);
            var nd = new byte[9];
            new Random().NextBytes(nd);

            v.HighestVersion = Peer.ChainParams.Version;
            v.Services = Peer.ChainParams.Services;
            v.Nonce = BitConverter.ToUInt64(nd, 0);
            v.RecvIp = ((IPEndPoint)Peer.RemoteEndpoint).Address;
            v.RecvPort = (UInt16)((IPEndPoint)Peer.RemoteEndpoint).Port;
            v.RecvServices = 0;
            v.StartHeight = 0;
            v.Timestamp = (UInt64)DateTimeOffset.Now.ToUnixTimeSeconds();
            v.TransIp = IPAddress.None;
            v.TransPort = 0;
            v.TransServices = Peer.ChainParams.Services;
            v.Relay = true;

            await WriteMessage(v);
        }

        public async Task SendAddr()
        {
            var a = new Addr();
            a.Ips = BlockChain.Peers.Values.Select(b => b.ToIP()).ToArray();

            await WriteMessage(a);
        }

        private async Task Peer_OnMessage(BitcoinPeer s, IStreamable msg)
        {
            switch (msg)
            {
                case bitcoin_lib.P2P.Version a:
                    {
                        PeerVersion = a;

                        if (Peer.IsInbound)
                        {
                            await SendVersion();
                        }

                        var va = new VerAck();
                        await s.WriteMessage(va);

                        if (Peer.IsInbound && PeerVersion.HighestVersion >= 70014)
                        {
                            //send cmpct (i only want my version, i dont care about another version)
                            //no version 1 msg will be sent if version 2 is supported
                            var nVersion = (Peer.ChainParams.Services & (ulong)Services.NODE_WITNESS) != 0 ? 2ul : 1ul;

                            var cmp = new SendCMPCT();
                            cmp.Version = nVersion;
                            cmp.Enabled = true;

                            await s.WriteMessage(cmp);
                        }

                        if (BlockChain.Mempool.Count == 0)
                        {
                            //ask for mempool
                            var mp = new MemPool();
                            await s.WriteMessage(mp);
                        }

                        var ph = ((IPEndPoint)Peer.RemoteEndpoint).AsHash();
                        if (BlockChain.Peers.ContainsKey(ph))
                        {
                            //update peer info
                            BlockChain.Peers[ph].LastSeen = DateTime.Now;
                            BlockChain.Peers[ph].LastVersion = a;
                        }
                        else
                        {
                            //ask the peer for addr info if this is the first time we seen them
                            var ga = new GetAddr();
                            await s.WriteMessage(ga);
                        }
                        break;
                    }
                case Inv a:
                    {
                        var gd = new GetData();
                        var to_get = new List<Inventory>();

                        var sw = new Stopwatch();
                        sw.Start();

                        to_get.AddRange(a.Inventory.Where(b => b.Type == InventoryType.MSG_TX && !BlockChain.Mempool.ContainsKey(b.Hash)));
                        if((PeerVersion.Services & (ulong)Services.NODE_WITNESS) == 1)
                        {
                            to_get.ForEach(b => b.Type = InventoryType.MSG_WITNESS_TX);
                        }
                        gd.Inventory = to_get.ToArray();

                        if (gd.Inventory.Length > 0)
                        {
                            //Console.WriteLine($"Asking for {gd.Count} tnxs, {sw.Elapsed.TotalMilliseconds.ToString("0.00")}ms");
                            await s.WriteMessage(gd);
                        }
                        break;
                    }
                case Tx a:
                    {
                        if (!BlockChain.Mempool.ContainsKey(a.Hash))
                        {
                            BlockChain.Mempool.Add(a.Hash, a);
                        }
                        break;
                    }
                case VerAck a:
                    {
                        if (Peer.IsInbound)
                        {
                            await SendAddr();
                        }

                        await s.WriteMessage(new Ping());
                        break;
                    }
                case Addr a:
                    {
                        foreach (var ip in a.Ips)
                        {
                            var ph = new IPEndPoint(ip.Ip, ip.Port).AsHash();
                            if (!BlockChain.Peers.ContainsKey(ph))
                            {
                                BlockChain.Peers.Add(ph, new PeerInfo()
                                {
                                    Ip = Peer.RemoteEndpoint,
                                    LastSeen = DateTime.Now,
                                    LastVersion = new bitcoin_lib.P2P.Version() //add empty version
                                });
                            }
                        }
                        break;
                    }
                case Pong a:
                    {
                        if (PingTimer.IsRunning)
                        {
                            PingTimer.Stop();
                        }

                        //Console.WriteLine($"[{RemoteEndpoint}]{PeerVersion.UserAgent} ping is: {LastPing.TotalMilliseconds.ToString("0.00")}ms");
                        break;
                    }
                case Ping a:
                    {
                        var pong = new Pong();
                        pong.Nonce = a.Nonce;

                        await s.WriteMessage(pong);
                        break;
                    }
                case MemPool a:
                    {
                        var inv = new Inv();
                        inv.Inventory = BlockChain.Mempool.Keys.Select(b => new Inventory()
                        {
                            Type = InventoryType.MSG_TX,
                            Hash = b
                        }).ToArray();

                        if (inv.Inventory.Length > 0)
                        {
                            await s.WriteMessage(inv);
                        }
                        break;
                    }
                case SendCMPCT a:
                    {
                        if (CMPCTBlockVersion == 0 && a.Enabled && a.Version <= 2)
                        {
                            if (Peer.IsInbound)
                            {
                                //lock in cmpct
                                var nVersion = (Peer.ChainParams.Services & (ulong)Services.NODE_WITNESS) != 0 ? 2ul : 1ul;
                                if (a.Version == nVersion)
                                {
                                    CMPCTBlockVersion = a.Version;
                                    Console.WriteLine($"Locking version {a.Version} cmpct block");
                                }
                            }
                            else
                            {
                                //reply to cmpct negotiation, if the node advertises witness use version 2
                                var nVersion = (PeerVersion.Services & (ulong)Services.NODE_WITNESS) != 0 ? 2ul : 1ul;
                                if (a.Version == nVersion)
                                {
                                    CMPCTBlockVersion = a.Version;
                                }
                                else if (nVersion != a.Version && CMPCTBlockVersion == 0) //if they only sent version 1 & they advertise NODE_WITNESS, use this version
                                {
                                    CMPCTBlockVersion = a.Version;
                                }

                                var rsp = new SendCMPCT();
                                rsp.Version = CMPCTBlockVersion;
                                rsp.Enabled = true;

                                Console.WriteLine($"Sending version {rsp.Version} cmpct block");
                                await s.WriteMessage(rsp);
                            }
                            Console.WriteLine($"Peer is asking for version {a.Version} cmpct block");
                        }
                        break;
                    }
            }
        }

    }
}
