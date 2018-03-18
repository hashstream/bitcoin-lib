using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class SendHeaders : IStreamable, ICommand
    {
        public string Command => "sendheaders";

        public void ReadFromPayload(byte[] data, int offset)
        {

        }

        public byte[] ToArray()
        {
            return new byte[0];
        }
    }
}
