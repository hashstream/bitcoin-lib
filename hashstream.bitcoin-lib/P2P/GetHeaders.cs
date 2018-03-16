using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class GetHeaders : GetBlocks
    {
        public new string Command => "getheaders";
    }
}
