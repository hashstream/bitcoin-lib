using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace hashstream.bitcoin_node_lib
{
    public abstract class PeerHandler
    {
        public abstract void Init(BitcoinPeer p, Guid g);

        public abstract Task SendVersion();
    }
}
