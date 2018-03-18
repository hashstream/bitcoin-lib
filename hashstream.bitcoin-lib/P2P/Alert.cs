using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class Alert : IStreamable, ICommand
    {
        public string Command => "alert";

        public void ReadFromPayload(byte[] data, int offset)
        {

        }

        public byte[] ToArray()
        {
            return new byte[0];
        }
    }
}
