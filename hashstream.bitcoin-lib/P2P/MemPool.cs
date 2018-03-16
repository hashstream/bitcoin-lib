using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class MemPool : IStreamable, ICommand
    {
        public string Command => "mempool";

        public void ReadFromPayload(byte[] data, int offset)
        {

        }

        public byte[] ToArray()
        {
            return new byte[0];
        }
    }
}
