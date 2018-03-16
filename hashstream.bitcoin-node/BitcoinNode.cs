using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace hashstream.bitcoin_node_lib
{
    public class BitcoinNode
    {
        private Socket Sock { get; set; }
        private Task AcceptTask { get; set; }

        public BitcoinNode(IPEndPoint ip = null)
        {
            if(ip == null)
            {
                ip = new IPEndPoint(IPAddress.Any, 8333);
            }

            Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Sock.Bind(ip);
        }

        public void Start()
        {
            Sock.Listen(1024);
            AcceptTask = Accept();
        }

        private async Task Accept()
        {
            while (true)
            {
                var ns = await Sock.AcceptAsync();
                new BitcoinPeer(ns);
            }
        }
    }
}
