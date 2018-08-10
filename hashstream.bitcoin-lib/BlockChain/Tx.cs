using hashstream.bitcoin_lib.P2P;
using hashstream.bitcoin_lib.Script;
using System;
using System.Linq;

namespace hashstream.bitcoin_lib.BlockChain
{
    public class Tx : IStreamable
    {
        public static readonly int SERIALIZE_TRANSACTION_NO_WITNESS = 0x40000000;
        public static readonly Int32 CURRENT_VERSION = 2;
        public static readonly Int32 MAX_STANDARD_VERSION = 2;

        public Int32 Version { get; set; } = CURRENT_VERSION;
        public byte Flags { get; set; }
        public VarInt TxInCount { get; set; }
        public TxIn[] TxIn { get; set; }
        public VarInt TxOutCount { get; set; }
        public TxOut[] TxOut { get; set; }
        public UInt32 LockTime { get; set; }

        public bool AllowWitness => (CURRENT_VERSION & SERIALIZE_TRANSACTION_NO_WITNESS) == 0;

        public int Size => (Flags != 0 ? 10 : 8) + TxInCount.Size + TxIn.Sum(a => ((Flags & 1) == 1 && AllowWitness ? a.NetworkSize : a.Size)) + TxOutCount.Size + TxOut.Sum(a => a.Size);

        public Hash TxHash => new Hash(ToArray().SHA256d());

        public int ReadFromPayload(byte[] data, int offset)
        {
            var roffset = offset;

            Version = data.ReadInt32FromBuffer(ref roffset);

            //check for extended version (empty txin length)
            var extended = data[roffset] == 0 && data[roffset + 1] != 0 && AllowWitness;
            if (extended)
            {
                Flags = data[roffset + 1];
                roffset += 2;
            }

            TxInCount = data.ReadFromBuffer<VarInt>(ref roffset);
            
            TxIn = new TxIn[TxInCount];
            for (var x = 0; x < TxInCount; x++)
            {
                TxIn[x] = data.ReadFromBuffer<TxIn>(ref roffset);
            }

            TxOutCount = data.ReadFromBuffer<VarInt>(ref roffset);

            TxOut = new TxOut[TxOutCount];
            for (var x = 0; x < TxOutCount; x++)
            {
                TxOut[x] = data.ReadFromBuffer<TxOut>(ref roffset);
            }

            //read witness scripts
            if((Flags & 1) == 1 && AllowWitness)
            {
                foreach(var tx in TxIn)
                {
                    tx.WitnessScripts = data.ReadFromBuffer<WitnessScript>(ref roffset);
                }
            }

            LockTime = BitConverter.ToUInt32(data, roffset);

            return Size;
        }

        public byte[] ToArray()
        {
            Flags |= AllowWitness && HasWitness() ? (byte)1 : (byte)0;

            var woffset = 0;
            var ret = new byte[Size];

            ret.CopyAndIncr(BitConverter.GetBytes(Version), ref woffset);

            //use extended block format
            if (Flags != 0)
            {
                ret.CopyAndIncr(new byte[] { 0x00, Flags }, ref woffset);
            }

            ret.CopyAndIncr(TxInCount.ToArray(), ref woffset);
            
            foreach(var tin in TxIn)
            {
                ret.CopyAndIncr(tin.ToArray(), ref woffset);
            }

            ret.CopyAndIncr(TxOutCount.ToArray(), ref woffset);

            foreach(var tout in TxOut)
            {
                ret.CopyAndIncr(tout.ToArray(), ref woffset);
            }

            //serialize witness scripts
            if((Flags & 1) == 1 && AllowWitness)
            {
                foreach(var tin in TxIn)
                {
                    var s = tin.WitnessScripts ?? new WitnessScript();
                    ret.CopyAndIncr(s.ToArray(), ref woffset);
                }
            }

            ret.CopyAndIncr(BitConverter.GetBytes(LockTime), ref woffset);

            return ret;
        }

        public bool HasWitness()
        {
            return TxIn.Any(a => a.WitnessScripts != null);
        }
    }
}
