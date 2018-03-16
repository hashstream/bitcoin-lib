using hashstream.bitcoin_lib;
using hashstream.bitcoin_lib.P2P;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace hashstream.bitcoin_node_lib
{
    public class BitcoinPeer
    {
        private Socket Sock { get; set; }
        private NetworkStream Stream { get; set; }
        private Task ReadTask { get; set; }
        private bool Closing { get; set; }
        private bitcoin_lib.P2P.Version Version { get; set; }

        public BitcoinPeer(Socket s)
        {
            Sock = s;
            Stream = new NetworkStream(Sock);

            InitVersion();
        }

        public BitcoinPeer(IPEndPoint ip)
        {
            Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Sock.Connect(ip);
            Stream = new NetworkStream(Sock);

            InitVersion();
        }

        public BitcoinPeer(string ip, int port = 8333)
        {
            Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Sock.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
            Stream = new NetworkStream(Sock);

            InitVersion();
        }

        private void InitVersion()
        {
            Version = new bitcoin_lib.P2P.Version("/hashstream/");
            Version.HighestVersion = 70015;
            Version.Nonce = (UInt64)new Random().Next();

            var rip = (IPEndPoint)Sock.RemoteEndPoint;
            var ripv6 = rip.Address.MapToIPv6();
            Version.RecvIp = ripv6;
            Version.RecvPort = (UInt16)rip.Port;
            Version.RecvServices = 0;
            Version.Relay = true;
            Version.Services = (UInt64)Services.NODE_NETWORK;
            Version.StartHeight = 0;
            Version.Timestamp = (UInt64)DateTimeOffset.Now.ToUnixTimeSeconds();
            Version.TransIp = IPAddress.Loopback.MapToIPv6();
            Version.TransPort = 0;
            Version.TransServices = Version.Services;
        }

        public async Task Start()
        {
            ReadTask = ReadStream();
            await WriteMessage(Version);
        }

        private async Task ReadStream()
        {
            while (!Closing)
            {
                try
                {
                    //try to read a header 
                    var hdata = new byte[24];
                    await Stream.ReadAsyncExact(hdata, 0, hdata.Length);

                    var h = new MessageHeader();
                    h.ReadFromPayload(hdata, 0);
                    if(h != null)
                    {
                        //read the payload
                        var pl = new byte[h.PayloadSize];
                        await Stream.ReadAsyncExact(pl, 0, pl.Length);
                        bool checksumOk = false;

                        //verify hash
                        using (var sha = SHA256.Create())
                        {
                            var h1 = sha.ComputeHash(pl);
                            var h2 = sha.ComputeHash(h1);

                            checksumOk = h2[0] == h.Checksum[0] && h2[1] == h.Checksum[1] && h2[2] == h.Checksum[2] && h2[3] == h.Checksum[3];
                        }

                        if (checksumOk)
                        {
                            switch (h.Command)
                            {
                                case "version\0\0\0\0\0":
                                    {
                                        var v = new bitcoin_lib.P2P.Version("");
                                        v.ReadFromPayload(pl, 0);

                                        //send verack
                                        var va = new VerAck();
                                        await WriteMessage(va);
                                        break;
                                    }
                                case "ping\0\0\0\0\0\0\0\0":
                                    {
                                        //read ping
                                        var ping = new Ping();
                                        ping.ReadFromPayload(pl, 0);

                                        //return pong
                                        var pong = new Pong();
                                        pong.Nonce = ping.Nonce;

                                        await WriteMessage(pong);
                                        Console.WriteLine("Pong");
                                        break;
                                    }
                                case "inv\0\0\0\0\0\0\0\0\0":
                                    {
                                        //read inv 
                                        var inv = new Inv();
                                        inv.ReadFromPayload(pl, 0);

                                        foreach (var i in inv.Inventory)
                                        {
                                            Console.WriteLine($"Got {i.Type.ToString()}: {i.Hash}");
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        Console.WriteLine($"Got cmd: {h.Command}");
                                        //take the payload off the stream 
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            Closing = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public async Task WriteMessage<T>(T msg) where T : IStreamable, ICommand
        {
            var dt = MessageHeader.ToCommand(msg);
            await Stream.WriteAsync(dt, 0, dt.Length);
        }
    }
}
