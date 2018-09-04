using hashstream.bitcoin_lib;
using hashstream.bitcoin_lib.P2P;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;
using hashstream.bitcoin_lib.BlockChain;
using System.Threading;

#if NETCOREAPP2_1
using System.Buffers;
#endif

namespace hashstream.bitcoin_node_lib
{

    public class BitcoinPeer
    {
        public ChainParams ChainParams { get; private set; }
        public bool IsInbound { get; private set; }

        private Socket Sock { get; set; }
        private NetworkStream Stream { get; set; }
        private Task ReadTask { get; set; }
        private CancellationTokenSource Cts { get; set; } = new CancellationTokenSource();
        public IPEndPoint RemoteEndpoint => (IPEndPoint)(Cts.IsCancellationRequested ? default : Sock?.RemoteEndPoint);

        public delegate Task MessageEvent(BitcoinPeer s, IStreamable a);
        public delegate void StoppingEvent();
        public event MessageEvent OnMessage;
        public event StoppingEvent OnStopping;

        public BitcoinPeer(ChainParams cp, Socket s, bool isInbound = false)
        {
            ChainParams = cp;
            IsInbound = isInbound;
            Sock = s;
            Sock.LingerState.Enabled = false;
            Stream = new NetworkStream(Sock);
        }
        
        public BitcoinPeer(ChainParams cp)
        {
            ChainParams = cp;
            Sock = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Sock.LingerState.Enabled = false;
        }

        public void Start()
        {
            ReadTask = ReadStream();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        public async Task ConnectAsync(IPEndPoint ip)
        {
            await ConnectAsyncInternal(ip);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="FormatException"></exception>
        /// 
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        public async Task ConnectAsync(string ip, int port = 8333)
        {
            await ConnectAsyncInternal(new IPEndPoint(IPAddress.Parse(ip), port));
        }
        
        private async Task ConnectAsyncInternal(IPEndPoint ip)
        {
#if NETCOREAPP2_1
            await Sock.ConnectAsync(ip);
#else
            var mre = new TaskCompletionSource<bool>();
            var sca = new SocketAsyncEventArgs();
            sca.Completed += (s, e) =>
            {
                mre.SetResult(true);
            };

            if (Sock.ConnectAsync(sca))
            {
                await mre.Task;
            }
#endif
            Stream = new NetworkStream(Sock);
        }

        public void Stop()
        {
            OnStopping?.Invoke();

            Cts.Cancel();
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

            while (!Cts.IsCancellationRequested)
            {
                try
                {
                    //try to read a header 
#if NETCOREAPP2_1
                    var h = await mstream.ReadMessage<MessageHeader>(MessageHeader.StaticSize, Cts.Token);
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
            IStreamable cmd = null;

            switch (h.Command.TrimEnd('\0'))
            {
                case "addr":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<Addr>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<Addr>();
#endif
                        break;
                    }
                case "alert":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<Alert>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<Alert>();
#endif
                        break;
                    }
                case "feefilter":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<FeeFilter>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<FeeFilter>();
#endif
                        break;
                    }
                case "filteradd":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<FilterAdd>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<FilterAdd>();
#endif
                        break;
                    }
                case "filterclear":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<FilterClear>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<FilterClear>();
#endif
                        break;
                    }
                case "filterload":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<FilterLoad>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<FilterLoad>();
#endif
                        break;
                    }
                case "getaddr":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<GetAddr>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<GetAddr>();
#endif
                        break;
                    }
                case "getblocks":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<GetBlocks>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<GetBlocks>();
#endif
                        break;
                    }
                case "getdata":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<GetData>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<GetData>();
#endif
                        break;
                    }
                case "getheaders":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<GetHeaders>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<GetHeaders>();
#endif
                        break;
                    }
                case "headers":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<Headers>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<Headers>();
#endif
                        break;
                    }
                case "inv":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<Inv>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<Inv>();
#endif
                        break;
                    }
                case "mempool":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<MemPool>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<MemPool>();
#endif
                        break;
                    }
                case "notfound":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<NotFound>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<NotFound>();
#endif
                        break;
                    }
                case "ping":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<Ping>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<Ping>();
#endif
                        break;
                    }
                case "pong":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<Pong>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<Pong>();
#endif
                        break;
                    }
                case "reject":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<Reject>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<Reject>();
#endif
                        break;
                    }
                case "sendheaders":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<SendHeaders>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<SendHeaders>();
#endif
                        break;
                    }
                case "verack":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<VerAck>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<VerAck>();
#endif
                        break;
                    }
                case "version":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<bitcoin_lib.P2P.Version>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<bitcoin_lib.P2P.Version>();
#endif
                        break;
                    }
                case "tx":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<Tx>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<Tx>();
#endif
                        break;
                    }
                case "block":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<Block>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<Block>();
#endif
                        break;
                    }
                case "sendcmpct":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<SendCMPCT>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<SendCMPCT>();
#endif
                        break;
                    }
                case "cmpctblock":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<CMPCTBlock>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<CMPCTBlock>();
#endif
                        break;
                    }
                case "getblocktxn":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<GetBlockTxn>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<GetBlockTxn>();
#endif
                        break;
                    }
                case "blocktxn":
                    {
#if NETCOREAPP2_1
                        cmd = await ms.ReadMessage<BlockTxn>((int)h.PayloadSize, Cts.Token, h.Checksum);
#else
                        cmd = pl.ReadFromBuffer<BlockTxn>();
#endif
                        break;
                    }
                default:
                    {
                        Console.WriteLine($"Got unknown cmd: {h.Command}");
#if NETCOREAPP2_1
                        //read the empty message
                        cmd = await ms.ReadMessage<VerAck>((int)h.PayloadSize, Cts.Token, h.Checksum);
#endif
                        break;
                    }
            }

            if(cmd != default)
            {
                await OnMessage?.Invoke(this, cmd);
            }
            else
            {
                Stop();
            }
        }

        public async Task WriteMessage<T>(T msg) where T : IStreamable, ICommand
        {
#if NETCOREAPP2_1
            var dt = MessageHeader.ToCommand(msg, ChainParams);
            if (!dt.IsEmpty && !Cts.IsCancellationRequested)
            {
                await Stream.WriteAsync(dt);
            }
#else
            var dt = MessageHeader.ToCommand(msg, ChainParams);
            if (dt != null && !Cts.IsCancellationRequested)
            {
                Console.WriteLine($"Sent cmd: {msg.Command}");
                await Stream.WriteAsync(dt, 0, dt.Length);
            }
#endif
        }
    }
}
