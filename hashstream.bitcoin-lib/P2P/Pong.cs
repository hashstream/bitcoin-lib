using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class Pong : Ping, ICommand
    {
        public new string Command => "pong";
    }
}
