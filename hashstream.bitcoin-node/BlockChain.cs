using hashstream.bitcoin_lib.BlockChain;
using System.Threading.Tasks;

namespace hashstream.bitcoin_node
{
    public static class BlockChain
    {
        public static HashStore<Tx> Mempool { get; set; }

        public static HashStore<PeerInfo> Peers { get; set; }

        public static void Init()
        {
            Mempool = new HashStore<Tx>("mempool");
            Peers = new HashStore<PeerInfo>("peers");
        }

        public static async Task FlushAll()
        {
            await Mempool.Flush();
            await Peers.Flush();
        }
    }
}
