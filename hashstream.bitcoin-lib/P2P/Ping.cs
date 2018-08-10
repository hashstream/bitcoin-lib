using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class Ping : IStreamable, ICommand
    {
        public UInt64 Nonce { get; set; }

        public string Command => "ping";

        public static int Size = 8;

        public Ping()
        {
            var nonce = new byte[8];
            new Random().NextBytes(nonce);
            Nonce = BitConverter.ToUInt64(nonce, 0);
        }

        public int ReadFromPayload(byte[] data, int offset)
        {
            Nonce = BitConverter.ToUInt64(data, offset);

            return Size;
        }

        public byte[] ToArray()
        {
            return BitConverter.GetBytes(Nonce);
        }

    }
}
