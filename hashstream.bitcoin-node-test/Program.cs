using hashstream.bitcoin_node_lib;
using System;
using System.Net;
using System.Net.Sockets;

namespace hashstream.bitcoin_node_test
{
    class Program
    {
        static void Main(string[] args)
        {
            var node = new BitcoinPeer("157.13.61.147", 8333);
            node.Start().Wait();
            Console.ReadKey();
        }
    }
}
