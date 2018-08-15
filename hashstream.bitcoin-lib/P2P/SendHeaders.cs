﻿using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class SendHeaders : IStreamable, ICommand
    {
        public string Command => "sendheaders";

        public int Size => 0;

#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            return data;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest;
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];
            WriteToPayload(ret);
            return ret;
        }
#else
        public int ReadFromPayload(byte[] data, int offset)
        {
            return 0;
        }

        public byte[] ToArray()
        {
            return new byte[0];
        }
#endif
    }
}
