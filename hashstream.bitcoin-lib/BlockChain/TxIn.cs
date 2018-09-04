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

        public int Size => Previous.Size + 4 + Script.Size;

        //this is only used by Tx serializer, witness scripts are at the end of the Tx buffer
        public int NetworkSize => Size + (WitnessScripts != null ? WitnessScripts.Size : 0);
        
#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var ret = data.ReadAndSlice(out Outpoint tPrev)
                .ReadAndSlice(out StandardScript tScript)
                .ReadAndSlice(out UInt32 tSeq);

            Previous = tPrev;
            Script = tScript;
            Sequence = tSeq;

            return ret;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(Previous)
                .WriteAndSlice(Script)
                .WriteAndSlice(Sequence);
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];
            WriteToPayload(ret);
            return ret;
        }
#else
        public int ReadFromPayload(byte[] data, int offset = 0)
        {
            var roffset = offset;
            Previous = data.ReadFromBuffer<Outpoint>(ref roffset);
            Script = data.ReadFromBuffer<StandardScript>(ref roffset);
            Sequence = data.ReadUInt32FromBuffer(ref roffset);

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];

            ret.CopyAndIncr(Previous.ToArray(), ref woffset);
            ret.CopyAndIncr(Script.ToArray(), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(Sequence), ref woffset);

            return ret;
        }
#endif

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
