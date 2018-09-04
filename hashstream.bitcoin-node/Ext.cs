using hashstream.bitcoin_lib.BlockChain;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace hashstream.bitcoin_node
{
    public static class Ext
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Hash AsHash(this IPEndPoint ip)
        {
            var ret = new byte[32];
            var ipb = ip.Address.MapToIPv6().GetAddressBytes();

            Array.Copy(ipb, ret, ipb.Length);
            Array.Copy(BitConverter.GetBytes((UInt16)ip.Port), 0, ret, ipb.Length, 2);

            return new Hash(ret);
        }

    }
}
