using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class GetAddr : IStreamable, ICommand
    {
        public string Command => "getaddr";

        public void ReadFromPayload(byte[] data, int offset)
        {

        }

        public byte[] ToArray()
        {
            return new byte[0];
        }
    }
}
