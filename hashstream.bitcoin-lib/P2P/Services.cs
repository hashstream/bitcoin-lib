using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    [Flags]
    public enum Services
    {
        Unknown = 0x00,
        NODE_NETWORK = 0x01
    }
}
