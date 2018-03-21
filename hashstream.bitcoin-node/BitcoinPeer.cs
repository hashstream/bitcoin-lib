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

        public BitcoinPeer(Socket s)
        {
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
                    await Stream.ReadAsyncExact(hdata, 0, hdata.Length);

                    var h = new MessageHeader();
                    h.ReadFromPayload(hdata, 0);
                    if (h != null)
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
                                case "addr\0\0\0\0\0\0\0\0":
                                    {
                                        if (OnAddr != null)
                                        {
                                            var a = new Addr();
                                            a.ReadFromPayload(pl, 0);

                                            await OnAddr?.Invoke(this, a);
                                        }
                                        break;
                                    }
                                case "alert\0\0\0\0\0\0\0":
                                    {
                                        if (OnAlert != null)
                                        {
                                            var a = new Alert();
                                            a.ReadFromPayload(pl, 0);

                                            await OnAlert?.Invoke(this, a);
                                        }
                                        break;
                                    }
                                case "feefilter\0\0\0":
                                    {
                                        if (OnFeeFilter != null)
                                        {
                                            var f = new FeeFilter();
                                            f.ReadFromPayload(pl, 0);

                                            await OnFeeFilter?.Invoke(this, f);
                                        }
                                        break;
                                    }
                                case "filteradd\0\0\0":
                                    {
                                        if (OnFilterAdd != null)
                                        {
                                            var f = new FilterAdd();
                                            f.ReadFromPayload(pl, 0);

                                            await OnFilterAdd?.Invoke(this, f);
                                        }
                                        break;
                                    }
                                case "filterclear\0":
                                    {
                                        if (OnFilterClear != null)
                                        {
                                            var f = new FilterClear();
                                            f.ReadFromPayload(pl, 0);

                                            await OnFilterClear?.Invoke(this, f);
                                        }
                                        break;
                                    }
                                case "filterload\0\0":
                                    {
                                        if (OnFilterLoad != null)
                                        {
                                            var f = new FilterLoad();
                                            f.ReadFromPayload(pl, 0);

                                            await OnFilterLoad?.Invoke(this, f);
                                        }
                                        break;
                                    }
                                case "getaddr\0\0\0\0\0":
                                    {
                                        if (OnGetAddr != null)
                                        {
                                            var ga = new GetAddr();
                                            ga.ReadFromPayload(pl, 0);

                                            await OnGetAddr?.Invoke(this, ga);
                                        }
                                        break;
                                    }
                                case "getblocks\0\0\0":
                                    {
                                        if (OnGetBlocks != null)
                                        {
                                            var gb = new GetBlocks();
                                            gb.ReadFromPayload(pl, 0);

                                            await OnGetBlocks?.Invoke(this, gb);
                                        }
                                        break;
                                    }
                                case "getdata\0\0\0\0\0":
                                    {
                                        if (OnGetData != null)
                                        {
                                            var gd = new GetData();
                                            gd.ReadFromPayload(pl, 0);

                                            await OnGetData?.Invoke(this, gd);
                                        }
                                        break;
                                    }
                                case "getheaders\0\0":
                                    {
                                        if (OnGetHeaders != null)
                                        {
                                            var gh = new GetHeaders();
                                            gh.ReadFromPayload(pl, 0);

                                            await OnGetHeaders?.Invoke(this, gh);
                                        }
                                        break;
                                    }
                                case "headers\0\0\0\0\0":
                                    {
                                        if (OnHeaders != null)
                                        {
                                            var hd = new Headers();
                                            hd.ReadFromPayload(pl, 0);

                                            await OnHeaders?.Invoke(this, hd);
                                        }
                                        break;
                                    }
                                case "inv\0\0\0\0\0\0\0\0\0":
                                    {
                                        if (OnInv != null)
                                        {
                                            var iv = new Inv();
                                            iv.ReadFromPayload(pl, 0);

                                            await OnInv?.Invoke(this, iv);
                                        }
                                        break;
                                    }
                                case "mempool\0\0\0\0\0":
                                    {
                                        if (OnMemPool != null)
                                        {
                                            var mp = new MemPool();
                                            mp.ReadFromPayload(pl, 0);

                                            await OnMemPool?.Invoke(this, mp);
                                        }
                                        break;
                                    }
                                case "notfound\0\0\0\0":
                                    {
                                        if (OnNotFound != null)
                                        {
                                            var nf = new NotFound();
                                            nf.ReadFromPayload(pl, 0);

                                            await OnNotFound?.Invoke(this, nf);
                                        }
                                        break;
                                    }
                                case "ping\0\0\0\0\0\0\0\0":
                                    {
                                        if (OnPing != null)
                                        {
                                            var ping = new Ping();
                                            ping.ReadFromPayload(pl, 0);

                                            await OnPing?.Invoke(this, ping);
                                        }
                                        break;
                                    }
                                case "pong\0\0\0\0\0\0\0\0":
                                    {
                                        if (OnPong != null)
                                        {
                                            var pong = new Pong();
                                            pong.ReadFromPayload(pl, 0);

                                            await OnPong?.Invoke(this, pong);
                                        }
                                        break;
                                    }
                                case "reject\0\0\0\0\0\0":
                                    {
                                        if (OnReject != null)
                                        {
                                            var re = new Reject();
                                            re.ReadFromPayload(pl, 0);

                                            await OnReject?.Invoke(this, re);
                                        }
                                        break;
                                    }
                                case "sendheaders\0":
                                    {
                                        if (OnSendHeaders != null)
                                        {
                                            var sh = new SendHeaders();
                                            sh.ReadFromPayload(pl, 0);

                                            await OnSendHeaders?.Invoke(this, sh);
                                        }
                                        break;
                                    }
                                case "verack\0\0\0\0\0\0":
                                    {
                                        if (OnVerAck != null)
                                        {
                                            var va = new VerAck();
                                            va.ReadFromPayload(pl, 0);

                                            await OnVerAck.Invoke(this, va);
                                        }
                                        break;
                                    } 
                                case "version\0\0\0\0\0":
                                    {
                                        if (OnVersion != null)
                                        {
                                            var v = new bitcoin_lib.P2P.Version("");
                                            v.ReadFromPayload(pl, 0);

                                            await OnVersion?.Invoke(this, v);
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        //Console.WriteLine($"Got cmd: {h.Command}");
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
                await Stream.WriteAsync(dt, 0, dt.Length);
            }
        }
    }
}
