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

        public void ReadFromPayload(byte[] data, int offset)
        {
            Value = BitConverter.ToUInt64(data, offset);

            RedeemScript = new StandardScript();
            RedeemScript.ReadFromPayload(data, offset + 8);
        }

        public Address GetAddress()
        {
            return RedeemScript.GetAddress();
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];

            var v = BitConverter.GetBytes(Value);
            Array.Copy(v, 0, ret, 0, v.Length);

            var sc = RedeemScript.ToArray();
            Array.Copy(sc, 0, ret, 8, sc.Length);

            return ret;
        }
    }
}
