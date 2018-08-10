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
        public bool IsInbound { get; private set; }

        private Socket Sock { get; set; }
        private NetworkStream Stream { get; set; }
        private Task ReadTask { get; set; }
        private bool Closing { get; set; }
        public EndPoint RemoteEndpoint => Sock.RemoteEndPoint;

        public delegate Task AddrEvent(BitcoinPeer s, Addr a);
        public delegate Task AlertEvent(BitcoinPeer s, Alert a);
        public delegate Task FeeFilterEvent(BitcoinPeer s, FeeFilter f);
        public delegate Task FilterAddEvent(BitcoinPeer s, FilterAdd f);
        public delegate Task FilterClearEvent(BitcoinPeer s, FilterClear f);
        public delegate Task FilterLoadEvent(BitcoinPeer s, FilterLoad f);
        public delegate Task GetAddrEvent(BitcoinPeer s, GetAddr a);
        public delegate Task GetBlocksEvent(BitcoinPeer s, GetBlocks gb);
        public delegate Task GetDataEvent(BitcoinPeer s, GetData gd);
        public delegate Task GetHeadersEvent(BitcoinPeer s, GetHeaders gh);
        public delegate Task HeadersEvent(BitcoinPeer s, Headers h);
        public delegate Task InvEvent(BitcoinPeer s, Inv i);
        public delegate Task MemPoolEvent(BitcoinPeer s, MemPool mp);
        public delegate Task NotFoundEvent(BitcoinPeer s, NotFound nf);
        public delegate Task PingEvent(BitcoinPeer s, Ping p);
        public delegate Task PongEvent(BitcoinPeer s, Pong p);
        public delegate Task RejectEvent(BitcoinPeer s, Reject r);
        public delegate Task SendHeadersEvent(BitcoinPeer s, SendHeaders sh);
        public delegate Task VerAckEvent(BitcoinPeer s, VerAck va);
        public delegate Task VersionEvent(BitcoinPeer s, bitcoin_lib.P2P.Version v);

        public event AddrEvent OnAddr;
        public event AlertEvent OnAlert;
        public event FeeFilterEvent OnFeeFilter;
        public event FilterAddEvent OnFilterAdd;
        public event FilterClearEvent OnFilterClear;
        public event FilterLoadEvent OnFilterLoad;
        public event GetAddrEvent OnGetAddr;
        public event GetBlocksEvent OnGetBlocks;
        public event GetDataEvent OnGetData;
        public event GetHeadersEvent OnGetHeaders;
        public event HeadersEvent OnHeaders;
        public event InvEvent OnInv;
        public event MemPoolEvent OnMemPool;
        public event NotFoundEvent OnNotFound;
        public event PingEvent OnPing;
        public event PongEvent OnPong;
        public event RejectEvent OnReject;
        public event SendHeadersEvent OnSendHeaders;
        public event VerAckEvent OnVerAck;
        public event VersionEvent OnVersion;

        public BitcoinPeer(Socket s, bool isInbound = false)
        {
            IsInbound = isInbound;
            Sock = s;
            Sock.LingerState.Enabled = false;
            Stream = new NetworkStream(Sock);
        }

        public BitcoinPeer(IPEndPoint ip)
        {
            Sock = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Sock.LingerState.Enabled = false;
            
            Sock.Connect(ip);
            Stream = new NetworkStream(Sock);
        }

        public BitcoinPeer(string ip, int port = 8333)
        {
            Sock = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Sock.LingerState.Enabled = false;
            Sock.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
            Stream = new NetworkStream(Sock);
        }
        
        public void Start()
        {
            ReadTask = ReadStream();
        }


        public void Stop()
        {
            Closing = true;
            Stream.Close();
            Sock.Close();

            Stream.Dispose();
            Sock.Dispose();
        }

        private async Task ReadStream()
        {
            while (!Closing)
            {
                try
                {
                    //try to read a header 
                    var hdata = new byte[24];
                    var rlen = await Stream.ReadAsyncExact(hdata, 0, hdata.Length);
                    if(rlen == 0)
                    {
                        Stop();
                        break;
                    }

                    var h = new MessageHeader();
                    h.ReadFromPayload(hdata, 0);
                    if (h != null)
                    {
                        //read the payload
                        var pl = new byte[h.PayloadSize];
                        await Stream.ReadAsyncExact(pl, 0, pl.Length);
                        var r0 = 0;

                        //verify hash
                        var h2 = pl.SHA256d();

                        if (h2[0] == h.Checksum[0] && h2[1] == h.Checksum[1] && h2[2] == h.Checksum[2] && h2[3] == h.Checksum[3])
                        {
                            Console.WriteLine($"Got cmd: {h.Command}");

                            switch (h.Command)
                            {
                                case "addr\0\0\0\0\0\0\0\0":
                                    {
                                        var a = pl.ReadFromBuffer<Addr>();
                                        await OnAddr?.Invoke(this, a);
                                        break;
                                    }
                                case "alert\0\0\0\0\0\0\0":
                                    {
                                        var a = pl.ReadFromBuffer<Alert>();
                                        await OnAlert?.Invoke(this, a);
                                        break;
                                    }
                                case "feefilter\0\0\0":
                                    {
                                        var f = pl.ReadFromBuffer<FeeFilter>();
                                        await OnFeeFilter?.Invoke(this, f);
                                        break;
                                    }
                                case "filteradd\0\0\0":
                                    {
                                        var f = pl.ReadFromBuffer<FilterAdd>();
                                        await OnFilterAdd?.Invoke(this, f);
                                        break;
                                    }
                                case "filterclear\0":
                                    {
                                        var f = pl.ReadFromBuffer<FilterClear>();
                                        await OnFilterClear?.Invoke(this, f);
                                        break;
                                    }
                                case "filterload\0\0":
                                    {
                                        var f = pl.ReadFromBuffer<FilterLoad>();
                                        await OnFilterLoad?.Invoke(this, f);
                                        break;
                                    }
                                case "getaddr\0\0\0\0\0":
                                    {
                                        var ga = pl.ReadFromBuffer<GetAddr>();
                                        await OnGetAddr?.Invoke(this, ga);
                                        break;
                                    }
                                case "getblocks\0\0\0":
                                    {
                                        var gb = pl.ReadFromBuffer<GetBlocks>();
                                        await OnGetBlocks?.Invoke(this, gb);
                                        break;
                                    }
                                case "getdata\0\0\0\0\0":
                                    {
                                        var gd = pl.ReadFromBuffer<GetData>();
                                        await OnGetData?.Invoke(this, gd);
                                        break;
                                    }
                                case "getheaders\0\0":
                                    {
                                        var gh = pl.ReadFromBuffer<GetHeaders>();
                                        await OnGetHeaders?.Invoke(this, gh);
                                        break;
                                    }
                                case "headers\0\0\0\0\0":
                                    {
                                        var hd = pl.ReadFromBuffer<Headers>();
                                        await OnHeaders?.Invoke(this, hd);
                                        break;
                                    }
                                case "inv\0\0\0\0\0\0\0\0\0":
                                    {
                                        var iv = pl.ReadFromBuffer<Inv>();
                                        await OnInv?.Invoke(this, iv);
                                        break;
                                    }
                                case "mempool\0\0\0\0\0":
                                    {
                                        var mp = pl.ReadFromBuffer<MemPool>();
                                        await OnMemPool?.Invoke(this, mp);
                                        break;
                                    }
                                case "notfound\0\0\0\0":
                                    {
                                        var nf = pl.ReadFromBuffer<NotFound>();
                                        await OnNotFound?.Invoke(this, nf);
                                        break;
                                    }
                                case "ping\0\0\0\0\0\0\0\0":
                                    {
                                        var ping = pl.ReadFromBuffer<Ping>();
                                        await OnPing?.Invoke(this, ping);
                                        break;
                                    }
                                case "pong\0\0\0\0\0\0\0\0":
                                    {
                                        var pong = pl.ReadFromBuffer<Pong>();
                                        await OnPong?.Invoke(this, pong);
                                        break;
                                    }
                                case "reject\0\0\0\0\0\0":
                                    {
                                        var re = pl.ReadFromBuffer<Reject>();
                                        await OnReject?.Invoke(this, re);
                                        break;
                                    }
                                case "sendheaders\0":
                                    {
                                        var sh = pl.ReadFromBuffer<SendHeaders>();
                                        await OnSendHeaders?.Invoke(this, sh);
                                        break;
                                    }
                                case "verack\0\0\0\0\0\0":
                                    {
                                        var va = pl.ReadFromBuffer<VerAck>();
                                        await OnVerAck?.Invoke(this, va);
                                        break;
                                    } 
                                case "version\0\0\0\0\0":
                                    {
                                        var v = pl.ReadFromBuffer<bitcoin_lib.P2P.Version>();
                                        await OnVersion?.Invoke(this, v);
                                        break;
                                    }
                                default:
                                    {
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
                    Closing = true;
                }
            }
        }

        public async Task WriteMessage<T>(T msg) where T : IStreamable, ICommand
        {
            var dt = MessageHeader.ToCommand(msg);
            if (dt != null)
            {
                Console.WriteLine($"Sent cmd: {msg.Command}");
                await Stream.WriteAsync(dt, 0, dt.Length);
            }
            else
            {
                Console.WriteLine("GOT NULL MSG");
            }
        }
    }
}
