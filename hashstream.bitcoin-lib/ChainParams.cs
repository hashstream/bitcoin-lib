using System;

namespace hashstream.bitcoin_lib
{
    public class ChainParams
    {
        public string UserAgent { get; set; } = "/hashstream.bitcoin-lib:1.0/";

        public UInt32 NetMagic { get; set; } = 0xd9b4bef9;

        public UInt32 Version { get; set; } = 70015;

        public UInt64 FeeRate { get; set; } = 100;

        public UInt64 Services { get; set; } = (UInt64)(P2P.Services.NODE_NETWORK | P2P.Services.NODE_WITNESS);
    }
}
