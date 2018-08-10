﻿namespace hashstream.bitcoin_lib.P2P
{
    public class VerAck : IStreamable, ICommand
    {
        public string Command => "verack";

        public int ReadFromPayload(byte[] data, int offset)
        {
            return 0;
        }

        public byte[] ToArray()
        {
            return new byte[0];
        }
    }
}
