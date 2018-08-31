using hashstream.bitcoin_lib;
using hashstream.bitcoin_lib.P2P;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;

#if NETCOREAPP2_1
using System.Buffers;
#endif

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
#if NETCOREAPP2_1
            var mstream = new MessageStream(Stream);
#else
            var hdata = new byte[MessageHeader.StaticSize];
#endif

            while (!Closing)
            {
                try
                {
                    //try to read a header 
#if NETCOREAPP2_1
                    var h = await mstream.ReadMessage<MessageHeader>(MessageHeader.StaticSize);
                    if(h == default)
                    {
                        Stop();
                        break;
                    }
#else
                    var rlen = await Stream.ReadAsyncExact(hdata, 0, hdata.Length); 
                    if (rlen == 0)
                    {
                        Stop();
                        break;
                    }
                    var h = new MessageHeader();
                    h.ReadFromPayload(hdata, 0);
#endif
                    if (h != null)
                    {
                        //read the payload
#if NETCOREAPP2_1
                        await HandleCommand(h, mstream);
#else
                        var pl = new byte[h.PayloadSize];
                        await Stream.ReadAsyncExact(pl, 0, pl.Length);
                        //verify hash
                        var h_chk = BitConverter.ToUInt32(pl.SHA256d(), 0);
                        if (h_chk == h.Checksum)
                        {
                            throw new Exception("Checksum failed");
                        }

                        await HandleCommand(h, pl);
#endif

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

#if NETCOREAPP2_1
        private async Task HandleCommand(MessageHeader h, MessageStream ms)
#else
        private async Task HandleCommand(MessageHeader h, byte[] pl)
#endif
        {
            Console.WriteLine($"Got cmd: {h.Command}");

            switch (h.Command)
            {
                case "addr\0\0\0\0\0\0\0\0":
                    {
#if NETCOREAPP2_1
                        var a = await ms.ReadMessage<Addr>((int)h.PayloadSize, h.Checksum);
#else
                        var a = pl.ReadFromBuffer<Addr>();
#endif
                        await OnAddr?.Invoke(this, a);
                        break;
                    }
                case "alert\0\0\0\0\0\0\0":
                    {
#if NETCOREAPP2_1
                        var a = await ms.ReadMessage<Alert>((int)h.PayloadSize, h.Checksum);
#else
                        var a = pl.ReadFromBuffer<Alert>();
#endif
                        await OnAlert?.Invoke(this, a);
                        break;
                    }
                case "feefilter\0\0\0":
                    {
#if NETCOREAPP2_1
                        var a = await ms.ReadMessage<FeeFilter>((int)h.PayloadSize, h.Checksum);
#else
                        var a = pl.ReadFromBuffer<FeeFilter>();
#endif
                        await OnFeeFilter?.Invoke(this, a);
                        break;
                    }
                case "filteradd\0\0\0":
                    {
#if NETCOREAPP2_1
                        var a = await ms.ReadMessage<FilterAdd>((int)h.PayloadSize, h.Checksum);
#else
                        var a = pl.ReadFromBuffer<FilterAdd>();
#endif
                        await OnFilterAdd?.Invoke(this, a);
                        break;
                    }
                case "filterclear\0":
                    {
#if NETCOREAPP2_1
                        var a = await ms.ReadMessage<FilterClear>((int)h.PayloadSize, h.Checksum);
#else
                        var a = pl.ReadFromBuffer<FilterClear>();
#endif
                        await OnFilterClear?.Invoke(this, a);
                        break;
                    }
                case "filterload\0\0":
                    {
#if NETCOREAPP2_1
                        var a = await ms.ReadMessage<FilterLoad>((int)h.PayloadSize, h.Checksum);
#else
                        var a = pl.ReadFromBuffer<FilterLoad>();
#endif
                        await OnFilterLoad?.Invoke(this, a);
                        break;
                    }
                case "getaddr\0\0\0\0\0":
                    {
#if NETCOREAPP2_1
                        var a = await ms.ReadMessage<GetAddr>((int)h.PayloadSize, h.Checksum);
#else
                        var a = pl.ReadFromBuffer<GetAddr>();
#endif
                        await OnGetAddr?.Invoke(this, a);
                        break;
                    }
                case "getblocks\0\0\0":
                    {
#if NETCOREAPP2_1
                        var a = await ms.ReadMessage<GetBlocks>((int)h.PayloadSize, h.Checksum);
#else
                        var a = pl.ReadFromBuffer<GetBlocks>();
#endif
                        await OnGetBlocks?.Invoke(this, a);
                        break;
                    }
                case "getdata\0\0\0\0\0":
                    {
#if NETCOREAPP2_1
                        var a = await ms.ReadMessage<GetData>((int)h.PayloadSize, h.Checksum);
#else
                        var a = pl.ReadFromBuffer<GetData>();
#endif
                        await OnGetData?.Invoke(this, a);
                        break;
                    }
                case "getheaders\0\0":
                    {
#if NETCOREAPP2_1
                        var a = await ms.ReadMessage<GetHeaders>((int)h.PayloadSize, h.Checksum);
#else
                        var a = pl.ReadFromBuffer<GetHeaders>();
#endif
                        await OnGetHeaders?.Invoke(this, a);
                        break;
                    }
                case "headers\0\0\0\0\0":
                    {
#if NETCOREAPP2_1
                        var a = await ms.ReadMessage<Headers>((int)h.PayloadSize, h.Checksum);
#else
                        var a = pl.ReadFromBuffer<Headers>();
#endif
                        await OnHeaders?.Invoke(this, a);
                        break;
                    }
                case "inv\0\0\0\0\0\0\0\0\0":
                    {
#if NETCOREAPP2_1
                        var a = await ms.ReadMessage<Inv>((int)h.PayloadSize, h.Checksum);
#else
                        var a = pl.ReadFromBuffer<Inv>();
#endif
                        await OnInv?.Invoke(this, a);
                        break;
                    }
                case "mempool\0\0\0\0\0":
                    {
#if NETCOREAPP2_1
                        var a = await ms.ReadMessage<MemPool>((int)h.PayloadSize, h.Checksum);
#else
                        var a = pl.ReadFromBuffer<MemPool>();
#endif
                        await OnMemPool?.Invoke(this, a);
                        break;
                    }
                case "notfound\0\0\0\0":
                    {
#if NETCOREAPP2_1
                        var a = await ms.ReadMessage<NotFound>((int)h.PayloadSize, h.Checksum);
#else
                        var a = pl.ReadFromBuffer<NotFound>();
#endif
                        await OnNotFound?.Invoke(this, a);
                        break;
                    }
                case "ping\0\0\0\0\0\0\0\0":
                    {
#if NETCOREAPP2_1
                        var a = await ms.ReadMessage<Ping>((int)h.PayloadSize, h.Checksum);
#else
                        var a = pl.ReadFromBuffer<Ping>();
#endif
                        await OnPing?.Invoke(this, a);
                        break;
                    }
                case "pong\0\0\0\0\0\0\0\0":
                    {
#if NETCOREAPP2_1
                        var a = await ms.ReadMessage<Pong>((int)h.PayloadSize, h.Checksum);
#else
                        var a = pl.ReadFromBuffer<Pong>();
#endif
                        await OnPong?.Invoke(this, a);
                        break;
                    }
                case "reject\0\0\0\0\0\0":
                    {
#if NETCOREAPP2_1
                        var a = await ms.ReadMessage<Reject>((int)h.PayloadSize, h.Checksum);
#else
                        var a = pl.ReadFromBuffer<Reject>();
#endif
                        await OnReject?.Invoke(this, a);
                        break;
                    }
                case "sendheaders\0":
                    {
#if NETCOREAPP2_1
                        var a = await ms.ReadMessage<SendHeaders>((int)h.PayloadSize, h.Checksum);
#else
                        var a = pl.ReadFromBuffer<SendHeaders>();
#endif
                        await OnSendHeaders?.Invoke(this, a);
                        break;
                    }
                case "verack\0\0\0\0\0\0":
                    {
#if NETCOREAPP2_1
                        var a = await ms.ReadMessage<VerAck>((int)h.PayloadSize, h.Checksum);
#else
                        var a = pl.ReadFromBuffer<VerAck>();
#endif
                        await OnVerAck?.Invoke(this, a);
                        break;
                    }
                case "version\0\0\0\0\0":
                    {
#if NETCOREAPP2_1
                        var a = await ms.ReadMessage<bitcoin_lib.P2P.Version>((int)h.PayloadSize, h.Checksum);
#else
                        var a = pl.ReadFromBuffer<bitcoin_lib.P2P.Version>();
#endif
                        await OnVersion?.Invoke(this, a);
                        break;
                    }
                default:
                    {
#if NETCOREAPP2_1
                        //read the empty message
                        var a = await ms.ReadMessage<VerAck>((int)h.PayloadSize, h.Checksum);
#endif
                        break;
                    }
            }
        }

        public async Task WriteMessage<T>(T msg) where T : IStreamable, ICommand
        {
#if NETCOREAPP2_1
            var dt = MessageHeader.ToCommand(msg);
            if (!dt.IsEmpty)
            {
                Console.WriteLine($"Sent cmd: {msg.Command}");
                await Stream.WriteAsync(dt);
            }
            else
            {
                Console.WriteLine("GOT NULL MSG");
            }
#else
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
#endif
        }
    }
}
