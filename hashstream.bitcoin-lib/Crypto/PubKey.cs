using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.Crypto
{
    public class PubKey
    {
        public static uint PUBLIC_KEY_SIZE => 65;
        public static uint COMPRESSED_PUBLIC_KEY_SIZE => 33;
        public static uint SIGNATURE_SIZE => 72;
        public static uint COMPACT_SIGNATURE_SIZE => 65;
    }
}
