using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class Ping : IStreamable
    {
        public UInt64 Nonce { get; set; }

        public string Command => "ping";

        public void ReadFromPayload(byte[] data, int offset)
        {
            Nonce = BitConverter.ToUInt64(data, offset);
        }

        public byte[] ToArray()
        {
            return BitConverter.GetBytes(Nonce);
        }

    }
}
