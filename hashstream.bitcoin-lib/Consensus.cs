using System;

namespace hashstream.bitcoin_lib
{
    public static class Consensus
    {
        public static UInt32 Version => 70015;

        public static UInt64 FeeRate => 100;

        public static UInt64 Services => (UInt64)P2P.Services.NODE_NETWORK;
    }
}
