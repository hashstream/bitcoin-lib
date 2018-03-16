using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_node_lib
{
    public class BlockChain
    {
        private ConnectionMultiplexer RedisConnection { get; set; }
        private IDatabase Db { get; set; }
        private string KeyPrefix { get; set; } = "hs";
        private string BlockList => K("chain");

        public BlockChain(string redis = "localhost")
        {
            RedisConnection = ConnectionMultiplexer.Connect(redis);
            Db = RedisConnection.GetDatabase();
        }

        private string K(string format, params object[] args)
        {
            return string.Format("{0}:{1}", KeyPrefix, string.Format(format, args));
        }
    }
}
