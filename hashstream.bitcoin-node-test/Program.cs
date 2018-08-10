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
            var node = new BitcoinNode(new IPEndPoint(IPAddress.Any, 8336));
            node.OnLog += Node_OnLog;
            node.OnPeerConnected += Node_OnPeerConnected;
            node.OnPeerDisconnected += Node_OnPeerDisconnected;
            
            node.Start();

            //node.AddPeer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12375)).RunSynchronously();
            
            Console.ReadKey();
        }

        private static void Node_OnPeerDisconnected(BitcoinNodePeer np)
        {
            
        }

        private static void Node_OnPeerConnected(BitcoinNodePeer np)
        {
            
        }

        private static void Node_OnLog(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
