using hashstream.bitcoin_lib.P2P;
using hashstream.bitcoin_node_lib;
using MaxMind.GeoIP2;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace hashstream.nodes
{
    public class NodeScraper
    {
        private static int MaxPoll => 50;

        private DatabaseReader GeoIp { get; set; }

        private ConnectionMultiplexer Redis { get; set; }

        private bool Running { get; set; } = true;

        private SemaphoreSlim NewNodeSemaphore { get; set; }

        private BufferBlock<IPEndPoint> NewNodeQueue { get; set; } = new BufferBlock<IPEndPoint>();

        private Task NewNodeWorkerTask { get; set; }
        
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
            NewNodeSemaphore = new SemaphoreSlim(MaxPoll, MaxPoll);
        }

        public async Task Run()
        {
            var db = Redis.GetDatabase();
            //var sub = Redis.GetSubscriber();
            //await sub.SubscribeAsync("hs:nodes:new-stream", NewNodeHandler);
            NewNodeWorkerTask = NewNodeWorker();

            if (!await db.KeyExistsAsync("hs:nodes:good-nodes"))
            {
                Console.WriteLine("First run.. checking seeds");
                //get seeds
                List<IPEndPoint> allIps = new List<IPEndPoint>();

                foreach (var dseed in DNSSeeds)
                {
                    var ips = await Dns.GetHostAddressesAsync(dseed);
                    if (ips != null && ips.Length > 0)
                    {
                        allIps.AddRange(ips.Select(a => new IPEndPoint(a.MapToIPv6(), 8333)));
                    }
                }

                foreach (var n in allIps)
                {
                    NewNodeQueue.Post(n);
                }

                Console.WriteLine("First run complete.");
            }

            while (Running)
            {
                if (NewNodeQueue.Count < 100)
                {
                    var s = new SemaphoreSlim(MaxPoll, MaxPoll);
                    Console.WriteLine("Scrape starting.");
                    var nodes = await GetGoodNodes();

                    foreach (var n in nodes)
                    {
                        await s.WaitAsync();
                        await Task.Factory.StartNew(async () =>
                        {
                            var nn = new Node(Redis, (ip) => { NewNodeQueue.Post(ip); }, GeoIp, n);
                            await nn.Connect();
                            s.Release();
                        });
                    }
                }
                else
                {
                    Console.WriteLine($"Skipping poll to catch up on {NewNodeQueue.Count} new nodes");
                }

                Console.WriteLine("Scrape complete.");
                await Task.Delay(TimeSpan.FromMinutes(5));
            }
        }

        private void NewNodeHandler(RedisChannel arg1, RedisValue arg2)
        {
            var ep = ((byte[])arg2).ParseIP();

            var nn = new Node(Redis, (ip) => { NewNodeQueue.Post(ip); }, GeoIp, ep);
            if (!nn.IsBadNode().Result && !nn.IsGoodNode().Result) //exclude already known nodes
            {
                NewNodeSemaphore.Wait();
                Console.WriteLine($"Checking new node {ep}");
                Task.Factory.StartNew(async () =>
                {
                    await nn.Connect();
                    NewNodeSemaphore.Release();
                });
            }
        }

        private async Task NewNodeWorker()
        {
            while (Running)
            {
                var ep = await NewNodeQueue.ReceiveAsync();

                var nn = new Node(Redis, (ip) => { NewNodeQueue.Post(ip); }, GeoIp, ep);
                if (!await nn.IsBadNode() && !await nn.IsGoodNode()) //exclude already known nodes
                {
                    await NewNodeSemaphore.WaitAsync();
                    Console.WriteLine($"Checking new node {ep}");
                    await Task.Factory.StartNew(async () =>
                    {
                        await nn.Connect();
                        NewNodeSemaphore.Release();
                    });
                }
            }
        }

        public async Task<List<IPEndPoint>> GetGoodNodes()
        {
            //hs:nodes:new-nodes
            var db = Redis.GetDatabase();

            var nodes = await db.SetMembersAsync("hs:nodes:good-nodes");
            return nodes.Select(a => ((byte[])a).ParseIP()).ToList();
        }
    }

    public class Node
    {
        public IPEndPoint IP { get; set; }
        public DateTime LastSeen { get; set; }

        private BitcoinPeer Peer { get; set; }
        private IDatabase db { get; set; }
        private DatabaseReader GeoIp { get; set; }
        private Action<IPEndPoint> QueueNewNode { get; set; }
        private bitcoin_lib.P2P.Version Ver { get; set; }
        private bool GotVersion { get; set; }

        public Node(ConnectionMultiplexer redis, Action<IPEndPoint> nq, DatabaseReader geo, IPEndPoint ip)
        {
            IP = ip;

            Ver = new bitcoin_lib.P2P.Version($"https://nodes.hashstream.net/");
            var nd = new byte[9];
            new Random().NextBytes(nd);
            Ver.Nonce = BitConverter.ToUInt64(nd, 0);
            Ver.RecvServices = 0;
            Ver.StartHeight = 0;
            Ver.TransIp = Ver.RecvIp = IPAddress.None;
            Ver.TransPort = Ver.RecvPort = 0;
            Ver.TransServices = Ver.RecvServices = (UInt64)Services.Unknown;
            Ver.Relay = false; //no need for them to send new tx / blocks since we only want to map the network

            QueueNewNode = nq;
            this.db = redis.GetDatabase();
            this.GeoIp = geo;
        }

        public void Close()
        {
            if(Peer != null)
            {
                Peer.Stop();
            }
        }

        public async Task Connect()
        {
            try
            {
                var ns = new Socket(SocketType.Stream, ProtocolType.Tcp);
#if NET461
                var sem_conn_net461 = new SemaphoreSlim(1);
                var sca = new SocketAsyncEventArgs();
                if (ns.ConnectAsync(sca))
                {
                    await sem_conn_net461.WaitAsync();
                }
#else
                await ns.ConnectAsync(IP);
#endif
                Peer = new BitcoinPeer(ns);
                Peer.OnVerAck += P_OnVerAck;
                Peer.OnVersion += P_OnVersion;
                Peer.OnAddr += P_OnAddr;
                Peer.OnPing += P_OnPing;
                Peer.Start();

                Ver.Timestamp = (UInt64)DateTimeOffset.Now.ToUnixTimeSeconds();

                await Peer.WriteMessage(Ver);
                
                //disconnect after 5s
                await Task.Factory.StartNew(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));

                    if (!GotVersion)
                    {
                        await SetAsBadNode();
                    }
                    Close();
                });
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Connection failed {ex.Message}, setting as bad node.");
                await SetAsBadNode();
            }
        }

        public async Task<bool> IsGoodNode()
        {
            var ipb = IP.ToByteArray();
            return await db.SetContainsAsync("hs:nodes:good-nodes", ipb);
        }

        public async Task<bool> SetAsGoodNode()
        {
            var ipb = IP.ToByteArray();
            await UnsetAsBadNode();
            return await db.SetAddAsync("hs:nodes:good-nodes", ipb);
        }

        public async Task<bool> UnsetAsGoodNode()
        {
            var ipb = IP.ToByteArray();
            return await db.SetRemoveAsync("hs:nodes:good-nodes", ipb);
        }

        public async Task<bool> IsBadNode()
        {
            var ipb = IP.ToByteArray();
            return await db.SetContainsAsync("hs:nodes:bad-nodes", ipb);
        }

        public async Task<bool> SetAsBadNode()
        {
            var ipb = IP.ToByteArray();
            await UnsetAsGoodNode();
            return await db.SetAddAsync("hs:nodes:bad-nodes", ipb);
        }

        public async Task<bool> UnsetAsBadNode()
        {
            var ipb = IP.ToByteArray();
            return await db.SetRemoveAsync("hs:nodes:bad-nodes", ipb);
        }

        public async Task<bool> SetNodeDetails(bitcoin_lib.P2P.Version v)
        {
            var ipb = IP.ToByteArray();
            await SetAsGoodNode();
            if(await db.HashSetAsync($"hs:nodes:detail:{IP.ToString()}", "version", v.ToArray()))
            {
                await db.HashSetAsync($"hs:nodes:detail:{IP.ToString()}", "first_seen", DateTime.Now.Ticks);

                var gloc = GeoIp.City(IP.Address);
                if (gloc.Location.HasCoordinates)
                {
                    await db.GeoAddAsync("hs:nodes:geo", new GeoEntry(gloc.Location.Longitude.Value, gloc.Location.Latitude.Value, ipb));
                }

                return true;
            }
            return false;
        }

        public async Task<bitcoin_lib.P2P.Version> GetNodeDetails()
        {
            var ret = new bitcoin_lib.P2P.Version("");
            var ipb = IP.ToByteArray();
            var rv = await db.HashGetAsync($"hs:nodes:detail:{IP.ToString()}", "version");
            if (rv.HasValue)
            {
#if NETCOREAPP2_1
                ret.ReadFromPayload(((byte[])rv).AsSpan());
#else
                ret.ReadFromPayload(rv, 0);
#endif
            }

            return ret;
        }

        private async Task P_OnAddr(BitcoinPeer s, Addr a)
        {
            foreach (var ip in a.Ips)
            {
                var ep = new IPEndPoint(ip.Ip.MapToIPv6(), ip.Port);
                var epb = ep.ToByteArray();
                if (!await db.SetContainsAsync("hs:nodes:good-nodes", epb) && !await db.SetContainsAsync("hs:nodes:bad-nodes", epb))
                {
                    QueueNewNode(ep);
                }
            }
        }

        private async Task P_OnPing(BitcoinPeer s, Ping p)
        {
            var pong = new Pong();
            pong.Nonce = p.Nonce;

            await s.WriteMessage(pong);
        }

        private async Task P_OnVersion(BitcoinPeer s, bitcoin_lib.P2P.Version v)
        {
            GotVersion = true;

            //send verack and log
            await SetNodeDetails(v);

            var va = new VerAck();
            await s.WriteMessage(va);

            var ga = new GetAddr();
            await s.WriteMessage(ga);
        }

        private Task P_OnVerAck(BitcoinPeer s, VerAck va)
        {
            return Task.CompletedTask;
        }
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
            Array.Copy(s, 0, ip, 0, ip.Length);
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
            Array.Copy(ipba, 0, ret, 0, ipba.Length);
            Array.Copy(portba, 0, ret, 16, portba.Length);

            return ret;
        }
    }
}
