using hashstream.bitcoin_lib.P2P;
using hashstream.bitcoin_lib.Script;
using System;

namespace hashstream.bitcoin_lib.BlockChain
{
    public class TxOut : IStreamable
    {
        public UInt64 Value { get; set; }
        public StandardScript RedeemScript { get; set; }

        public int Size => 8 + RedeemScript.Size;

        public int ReadFromPayload(byte[] data, int offset)
        {
            var roffset = offset;
            Value = data.ReadUInt64FromBuffer(ref roffset);
            RedeemScript = data.ReadFromBuffer<StandardScript>(ref roffset);

            return Size;
        }

        public Address GetAddress()
        {
            return RedeemScript.GetAddress();
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];

            ret.CopyAndIncr(BitConverter.GetBytes(Value), ref woffset);
            ret.CopyAndIncr(RedeemScript.ToArray(), woffset);

            return ret;
        }
    }
}
