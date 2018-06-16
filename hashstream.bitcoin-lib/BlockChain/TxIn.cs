using hashstream.bitcoin_lib.P2P;
using hashstream.bitcoin_lib.Script;
using System;

namespace hashstream.bitcoin_lib.BlockChain
{
    public class TxIn : IStreamable
    {
        public Outpoint Previous { get; set; }
        public StandardScript Script { get; set; }
        public WitnessScript WitnessScripts { get; set; }
        public UInt32 Sequence { get; set; } = 0xffffffff;

        public int Size => 40 + Script.Size;

        public void ReadFromPayload(byte[] data, int offset)
        {
            Previous = new Outpoint();
            Previous.ReadFromPayload(data, offset);

            Script = new StandardScript();
            Script.ReadFromPayload(data, offset + Previous.Size);

            Sequence = BitConverter.ToUInt32(data, (int)(offset + Previous.Size + Script.Size));
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];

            var p = Previous.ToArray();
            Array.Copy(p, 0, ret, 0, p.Length);

            var sc = Script.ToArray();
            Array.Copy(sc, 0, ret, p.Length, sc.Length);

            var sq = BitConverter.GetBytes(Sequence);
            Array.Copy(sq, 0, ret, p.Length + sc.Length, sq.Length);

            return ret;
        }

        public bool Verify(TxOut prev)
        {
            //if this is not the correct outpoint validation will fail anyway
            var redeemScript = prev.RedeemScript;
            var scriptSig = Script;

            if (scriptSig.IsPayToWitnessScriptHash())
            {

            }
            else if (scriptSig.IsWitnessProgram(out int version))
            {

            }

            return false;
        }
    }
}
