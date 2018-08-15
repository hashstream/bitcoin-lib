using hashstream.bitcoin_lib;
using hashstream.bitcoin_lib.BlockChain;
using hashstream.bitcoin_lib.P2P;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace hashstream.bitcoin_node_lib
{
    public class BlockChain
    {
        private ConnectionMultiplexer RedisConnection { get; set; }
        private IDatabase Db { get; set; }
        private string KeyPrefix { get; set; } = "hs";

        public BlockChain(string redis = "localhost", string chain = "bitcoin")
        {
            KeyPrefix = $"{KeyPrefix}:{chain}";
            RedisConnection = ConnectionMultiplexer.Connect(redis);
            Db = RedisConnection.GetDatabase();
        }

        private string K(string format, params object[] args)
        {
            return string.Format("{0}:{1}", KeyPrefix, string.Format(format, args));
        }

        private async Task<T> StringGetAsync<T>(RedisKey key) where T : IStreamable, new()
        {
            var v = await Db.StringGetAsync(key);
            if(v != RedisValue.Null)
            {
                var ret = new T();
#if NETCOREAPP2_1
                ret.ReadFromPayload(((byte[])v).AsSpan());
#else
                ret.ReadFromPayload(v, 0);
#endif

                return ret;
            }

            return default(T);
        }

        public async Task<BlockHeader[]> GetLastBlocksAsync(int x)
        {
            var bx = Db.SortedSetRangeByScore(K("chain"), Double.NegativeInfinity, Double.PositiveInfinity, Exclude.None, Order.Descending, 0, x);
            var ret = new BlockHeader[bx.Length];
            for(var y = 0; y < ret.Length; y++)
            {
                ret[y] = await GetHeaderAsync((byte[])bx[y]);
            }

            return ret;
        }

        public async Task<BlockHeader> GetHeaderAsync(Hash id)
        {
            return await StringGetAsync<BlockHeader>(K("block:{0}", id));
        }

        public async Task<Tx[]> GetBlockTxnAsync(Hash id)
        {
            var txn = await Db.SetMembersAsync(K("block:{0}:txn", id));

            var ret = new Tx[txn.Length];
            for(var x=0;x<ret.Length;x++)
            {
                ret[x] = await GetTxAsync(new Hash((byte[])txn[x]));
            }
            return ret;
        }

        public async Task<Tx> GetTxAsync(Hash id)
        {
            return await StringGetAsync<Tx>(K("tx:{0}", id));
        }
    }
}
