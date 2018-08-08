using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.BlockChain
{
    public enum AddressNetwork
    {
        Unknown,
        Main,
        Test,
        RegTest
    }

    public abstract class Address
    {
        public AddressNetwork Network { get; internal set; }

        public byte[] AddressBytes { get; internal set; }

        public abstract byte[] ToArray();
        public new abstract string ToString();
    }
}
