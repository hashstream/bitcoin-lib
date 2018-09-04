using hashstream.bitcoin_lib;
using hashstream.bitcoin_lib.P2P;
using hashstream.bitcoin_node_lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace hashstream.bitcoin_node
{
    class Program
    {
        private static DateTime LastPingSent { get; set; }
        private static BitcoinNode<BitcoinNodePeer> Node { get; set; }

        private static List<string> DNSSeeds { get; set; } = new List<string>()
        {
            "seed.bitcoin.sipa.be",
            "dnsseed.bluematt.me",
            "dnsseed.bitcoin.dashjr.org",
            "seed.bitcoinstats.com",
            "seed.bitcoin.jonasschnelli.ch",
            "seed.btc.petertodd.org"
        };

        static Task Main(string[] args)
        {
            return RunNode();
        }

        private static async Task RunNode()
        {
            try
            {
                BlockChain.Init();

                var cp_btc = new ChainParams();

                Node = new BitcoinNode<BitcoinNodePeer>(cp_btc, new IPEndPoint(IPAddress.Any, 8336));
                Node.OnLog += Node_OnLog;

                Node.Start();

                //var ct = node.AddPeer(new IPEndPoint(IPAddress.Parse("192.168.254.6"), 8333));
                //var ct2 = node.AddPeer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8333));

                //if no peers, add seeds
                if (BlockChain.Peers.Count == 0)
                {
                    Console.WriteLine("No peers found, adding seed nodes..");
                    foreach (var seed in DNSSeeds)
                    {
                        try
                        {
                            var ips = await Dns.GetHostAddressesAsync(seed);
                            foreach (var ip in ips)
                            {
                                var ep = new IPEndPoint(ip, 8333);
                                BlockChain.Peers.Add(ep.AsHash(), new PeerInfo()
                                {
                                    Ip = ep,
                                    LastSeen = DateTime.Now,
                                    LastVersion = new bitcoin_lib.P2P.Version()
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"No ips found for seed: {seed} ({ex.Message})");
                        }
                    }
                }

                //connect to last 8 peers
                await TryAddLastPeers();

                bool exit = false;
                Console.CancelKeyPress += (s, e) =>
                {
                    Console.WriteLine($"Shutting down..");
                    exit = true;

                    //doesnt work in .net core it seems..
                    //https://github.com/dotnet/coreclr/issues/8565
                };

                while (!exit)
                {
                    //ping the peers every 60s
                    if ((DateTime.Now - LastPingSent).TotalSeconds >= 60)
                    {
                        //disconnect a high ping node
                        BitcoinNodePeer hpn = Node.EnumeratePeers().Where(a => a.LastPing != TimeSpan.Zero).OrderByDescending(a => a.LastPing.TotalMilliseconds).FirstOrDefault();
                        if (hpn != default)
                        {
                            hpn.Disconnect();
                        }

                        var pt = new List<Task>();
                        foreach (var n in Node.EnumeratePeers())
                        {
                            pt.Add(n.StartPing());
                        }
                        await Task.WhenAll(pt);
                        LastPingSent = DateTime.Now;
                    }

                    //try to get 8 peers
                    if (Node.EnumeratePeers().Count() < 8)
                    {
                        //ask for more peers if we dont have enough
                        if (BlockChain.Peers.Count < 100)
                        {
                            Console.WriteLine($"Not enough known peers, asking for more..");
                            foreach (var n in Node.EnumeratePeers())
                            {
                                var addr = new GetAddr();
                                await n.WriteMessage(addr);
                            }

                            //wait 2s for nodes to reply
                            await Task.Delay(2000);
                        }

                        await TryAddLastPeers();
                    }

                    await Task.Delay(100);
                }

                await ShutdownNode();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static async Task ShutdownNode()
        {
            foreach (var n in Node.EnumeratePeers())
            {
                n.Disconnect();
            }

            Console.WriteLine($"Flushing mempool & peers..");
            await BlockChain.FlushAll();
        }


        private static async Task TryAddLastPeers()
        {
            var nc = new List<Task>();
            var nac = Node.EnumeratePeers().ToList();
            if (nac.Count < 8)
            {
                foreach (var p in BlockChain.Peers.Values.Reverse().Where(a => !nac.Any(b => b.Peer.RemoteEndpoint.AsHash() == a.Ip.AsHash())).Take(8 - nac.Count))
                {
                    nc.Add(TryAddPeer(p.Ip));
                }

                await Task.WhenAll(nc);
            }
        }

        private static async Task TryAddPeer(IPEndPoint ip)
        {
            try
            {
                await Node.AddPeer(ip);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to peer {ip} ({ex.Message})");
                //remove this peer if it cant be reached [sorry :(]
                var ph = ip.AsHash();
                if (BlockChain.Peers.ContainsKey(ph))
                {
                    BlockChain.Peers.Remove(ph);
                }
            }
        }

        private static void Node_OnPeerDisconnected(BitcoinNodePeer np)
        {

        }

        private static void Node_OnPeerConnected(BitcoinNodePeer np)
        {
            Console.WriteLine($"Client connected {np.PeerVersion.UserAgent} [{np.Peer.RemoteEndpoint}]");
        }

        private static void Node_OnLog(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
