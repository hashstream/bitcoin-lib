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
        public VarInt TxInCount => TxIn?.Length;
        public TxIn[] TxIn { get; set; } = new TxIn[0];
        public VarInt TxOutCount => TxOut?.Length;
        public TxOut[] TxOut { get; set; } = new TxOut[0];
        public UInt32 LockTime { get; set; }

        public bool AllowWitness => (CURRENT_VERSION & SERIALIZE_TRANSACTION_NO_WITNESS) == 0;

        public int Size => (Flags != 0 ? 10 : 8) + TxInCount.Size + TxIn.Sum(a => ((Flags & 1) == 1 && AllowWitness ? a.NetworkSize : a.Size)) + TxOutCount.Size + TxOut.Sum(a => a.Size);

        public Hash TxHash => new Hash(ToArray().SHA256d());

        public bool HasWitness()
        {
            return TxIn.Any(a => a.WitnessScripts != null);
        }

#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out Int32 tVersion);

            var extended = next[0] == 0 && next[1] != 0 && AllowWitness;
            if (extended)
            {
                Flags = next[1];
                next = next.Slice(2);
            }

            next = next.ReadAndSlice(out VarInt tTxInCount)
                .ReadAndSlice(tTxInCount, out TxIn[] tTxIn)
                .ReadAndSlice(out VarInt tTxOutCount)
                .ReadAndSlice(tTxOutCount, out TxOut[] tTxOut);

            if ((Flags & 1) == 1 && AllowWitness)
            {
                foreach (var tx in tTxIn)
                {
                    next = next.ReadAndSlice(out WitnessScript tWitnessScript);
                    tx.WitnessScripts = tWitnessScript;
                }
            }

            next = next.ReadAndSlice(out UInt32 tLocktime);

            Version = tVersion;
            TxIn = tTxIn;
            TxOut = tTxOut;
            LockTime = tLocktime;

            return next;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            Flags |= AllowWitness && HasWitness() ? (byte)1 : (byte)0;

            var ret = dest.WriteAndSlice(Version);

            //use extended block format
            if (Flags != 0)
            {
                new byte[] { 0x00, Flags }.AsSpan().CopyTo(ret);
                ret = ret.Slice(2);
            }

            ret = ret.WriteAndSlice(TxInCount)
                .WriteAndSlice(TxIn)
                .WriteAndSlice(TxOutCount)
                .WriteAndSlice(TxOut);

            //serialize witness scripts
            if ((Flags & 1) == 1 && AllowWitness)
            {
                foreach (var tin in TxIn)
                {
                    ret = ret.WriteAndSlice(tin.WitnessScripts ?? new WitnessScript());
                }
            }

            return ret.WriteAndSlice(LockTime);
        }

        public byte[] ToArray()
        {
            var dest = new byte[Size];
            WriteToPayload(dest);
            return dest;
        }
#else
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

            var txinc = data.ReadFromBuffer<VarInt>(ref roffset);
            
            TxIn = new TxIn[txinc];
            for (var x = 0; x < txinc; x++)
            {
                TxIn[x] = data.ReadFromBuffer<TxIn>(ref roffset);
            }

            var txoc = data.ReadFromBuffer<VarInt>(ref roffset);

            TxOut = new TxOut[txoc];
            for (var x = 0; x < txoc; x++)
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
#endif
    }
}
