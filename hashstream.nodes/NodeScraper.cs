using hashstream.bitcoin_lib.P2P;
using hashstream.bitcoin_node_lib;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace hashstream.nodes
{
    public class NodeScraper
    {
        private static int MaxPoll => 100;

        private MaxMind.GeoIP2.DatabaseReader GeoIp { get; set; }

        private ConnectionMultiplexer Redis { get; set; }

        private bitcoin_lib.P2P.Version Ver { get; set; }

        private bool Running { get; set; } = true;

        private SemaphoreSlim ParallelTasks { get; set; } = new SemaphoreSlim(MaxPoll, MaxPoll);

        private static List<string> DNSSeeds { get; set; } = new List<string>()
        {
            "seed.bitcoin.sipa.be",
            "dnsseed.bluematt.me",
            "dnsseed.bitcoin.dashjr.org",
            "seed.bitcoinstats.com",
            "seed.bitcoin.jonasschnelli.ch",
            "seed.btc.petertodd.org"
        };

        public NodeScraper()
        {
            ThreadPool.SetMinThreads(MaxPoll, 20);
            GeoIp = new MaxMind.GeoIP2.DatabaseReader("GeoLite2-City.mmdb");
            Redis = ConnectionMultiplexer.Connect("localhost");

            Ver = new bitcoin_lib.P2P.Version($"https://nodes.hashstream.net/");
            var nd = new byte[9];
            new Random().NextBytes(nd);
            Ver.Nonce = BitConverter.ToUInt64(nd, 0);
            Ver.RecvServices = 0;
            Ver.StartHeight = 0;
            Ver.TransIp = Ver.RecvIp = IPAddress.None;
            Ver.TransPort = Ver.RecvPort = 0;
            Ver.TransServices = Ver.RecvServices = (UInt64)Services.Unknown;
        }

        public async Task Run()
        {
            var db = Redis.GetDatabase();

            if (!await db.KeyExistsAsync("hs:nodes:all-nodes"))
            {
                Console.WriteLine("First run.. checking seeds");
                //get seeds
                List<Node> allIps = new List<Node>();

                foreach (var dseed in DNSSeeds)
                {
                    var ips = await Dns.GetHostAddressesAsync(dseed);
                    if (ips != null && ips.Length > 0)
                    {
                        allIps.AddRange(ips.Select(a => new Node()
                        {
                            IP = new IPEndPoint(a, 8333),
                            LastSeen = new DateTime()
                        }));
                    }
                }

                foreach (var n in allIps)
                {
                    await ParallelTasks.WaitAsync();
                    UpdateNodeInfo(n);

                    ParallelTasks.Release();
                }

                Console.WriteLine("First run complete.");
            }

            while (Running)
            {
                var nodes = await GetNewNodes();

                foreach (var n in nodes)
                {
                    await ParallelTasks.WaitAsync();
                    UpdateNodeInfo(n);
                    ParallelTasks.Release();
                }

                Console.WriteLine("Scrape complete.");
                await Task.Delay(30000);
            }
        }

        public async Task<List<Node>> GetNewNodes()
        {
            //hs:nodes:new-nodes
            var db = Redis.GetDatabase();

            var nodes = await db.SetMembersAsync("hs:nodes:new-nodes");
            return nodes.Select(a => new Node()
            {
                IP = ((byte[])a).ParseIP(),
                LastSeen = new DateTime()
            }).ToList();
        }

        public async Task<List<Node>> GetNodeList(string list)
        {
            var db = Redis.GetDatabase();

            var nodes = await db.SortedSetRangeByScoreWithScoresAsync($"hs:nodes:{list}", double.NegativeInfinity, double.PositiveInfinity, Exclude.None, Order.Descending);
            return nodes.Select(a => new Node()
            {
                IP = ((byte[])a.Element).ParseIP(),
                LastSeen = new DateTime((long)a.Score)
            }).ToList();
        }

        public async Task UpdateNodeInfo(Node n)
        {
            //Console.WriteLine($"Update node info: {n.IP}");
            try
            {
                //try to connect to the node to get version info and to get its peers
                var ns = new Socket(SocketType.Stream, ProtocolType.Tcp);
                await ns.ConnectAsync(n.IP);
                var p = new BitcoinPeer(ns);
                //Console.WriteLine($"Connected to {n.IP}");
                var ss = new SemaphoreSlim(0, 1);
                var db = Redis.GetDatabase();

                p.OnVersion += async (s, v) =>
                {
                    //send verack and log
                    var ip = new IPEndPoint(((IPEndPoint)s.RemoteEndpoint).Address.MapToIPv6(), ((IPEndPoint)s.RemoteEndpoint).Port);
                    var ipb = ip.ToByteArray();
                    await db.HashSetAsync($"hs:nodes:detail:{ip.ToString()}", "version", v.ToArray());
                    if (await db.SortedSetAddAsync("hs:nodes:all-nodes", ipb, DateTime.Now.Ticks))
                    {
                        //Console.WriteLine($"Got new node: {ip}");
                        await db.SetRemoveAsync("hs:nodes:new-nodes", ipb);
                        var gloc = GeoIp.City(ip.Address);
                        if (gloc.Location.HasCoordinates)
                        {
                            await db.GeoAddAsync("hs:nodes:geo", new GeoEntry(gloc.Location.Longitude.Value, gloc.Location.Latitude.Value, ipb));
                        }
                    }


                    var va = new VerAck();
                    await s.WriteMessage(va);

                    var ga = new GetAddr();
                    await s.WriteMessage(ga);
                };

                p.OnAddr += async (s, a) =>
                {
                    //Console.WriteLine($"Got {a.IpCount.Value} ips");
                    foreach (var ip in a.Ips)
                    {
                        var ep = new IPEndPoint(ip.Ip.MapToIPv6(), ip.Port);
                        var epb = ep.ToByteArray();
                        if (await db.SetAddAsync("hs:nodes:new-nodes", epb))
                        {
                            //Console.WriteLine($"Got new node: {ep}");
                        }
                    }
                    
                    s.Stop();

                    ss.Release();

                    //Console.WriteLine($"Disconnected from {n.IP}");
                };

                p.Start();

                Ver.Timestamp = (UInt64)DateTimeOffset.Now.ToUnixTimeSeconds();

                await p.WriteMessage(Ver);
                await ss.WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex);
            }
        }        
    }

    public class Node
    {
        public IPEndPoint IP { get; set; }
        public DateTime LastSeen { get; set; }
    }

    internal static class Ext
    {
        //small hack but better than string parsing?
        public static IPEndPoint ParseIP(this byte[] s)
        {
            if (s.Length != 18)
                return null;

            var ip = new byte[16];
            var port = 0;
            Buffer.BlockCopy(s, 0, ip, 0, ip.Length);
            port = BitConverter.ToUInt16(s, 16);

            return new IPEndPoint(new IPAddress(ip), port);
        }

        public static byte[] ToByteArray(this IPEndPoint ep)
        {
            var ip = ep.Address.MapToIPv6();
            var port = ep.Port;

            var ret = new byte[18];
            var ipba = ip.GetAddressBytes();
            var portba = BitConverter.GetBytes((UInt16)port);
            Buffer.BlockCopy(ipba, 0, ret, 0, ipba.Length);
            Buffer.BlockCopy(portba, 0, ret, 16, portba.Length);

            return ret;
        }
    }
}
