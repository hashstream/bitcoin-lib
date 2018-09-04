using System;
using System.Net;
using System.Threading.Tasks;

namespace hashstream.bitcoin_node_lib
{
    public delegate void PeerStoppingEvent(Guid g);

    public interface PeerHandler
    {
        event PeerStoppingEvent OnStop;

        void Init<T>(BitcoinNode<T> node, BitcoinPeer peer, Guid id) where T : PeerHandler, new();

        IPEndPoint RemoteEndpoint { get; }

        Task SendVersion();

        void Disconnect();
    }
}
