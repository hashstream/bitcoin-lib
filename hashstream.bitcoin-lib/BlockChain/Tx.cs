using hashstream.bitcoin_lib.P2P;
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
        public int Size { get; private set; }

        public void ReadFromPayload(byte[] data, int offset)
        {
            var allowWitness = (CURRENT_VERSION & SERIALIZE_TRANSACTION_NO_WITNESS) == 0;
            var readoffset = offset;

            Size = 8;
            Version = BitConverter.ToInt32(data, readoffset);
            readoffset += 4;

            //check for extended version
            var extended = data[readoffset] == 0 && data[readoffset + 1] != 0 && allowWitness;
            if (extended)
            {
                Flags = data[readoffset + 1];
                readoffset += 2;
                Size += 2;
            }

            TxInCount = new VarInt(0);
            TxInCount.ReadFromPayload(data, readoffset);

            Size += TxInCount.Size;
            readoffset += TxInCount.Size;

            TxIn = new TxIn[TxInCount];
            for (var x = 0; x < TxInCount; x++)
            {
                var tx = new TxIn();
                tx.ReadFromPayload(data, readoffset);

                TxIn[x] = tx;

                readoffset += tx.Size;
                Size += tx.Size;
            }

            TxOutCount = new VarInt(0);
            TxOutCount.ReadFromPayload(data, readoffset);

            Size += TxOutCount.Size;
            readoffset += TxOutCount.Size;

            TxOut = new TxOut[TxOutCount];
            for (var x = 0; x < TxOutCount; x++)
            {
                var tx = new TxOut();
                tx.ReadFromPayload(data, readoffset);

                TxOut[x] = tx;

                readoffset += tx.Size;
                Size += tx.Size;
            }

            //read witness scripts
            if((Flags & 1) == 1 && allowWitness)
            {
                Flags ^= 1;
                foreach(var tx in TxIn)
                {
                    //read the len
                    tx.WitnessScripts = new WitnessScripts();
                    tx.WitnessScripts.ReadFromPayload(data, readoffset);

                    readoffset += tx.WitnessScripts.TotalLength;
                    Size += tx.WitnessScripts.TotalLength;
                }
            }

            LockTime = BitConverter.ToUInt32(data, readoffset);
        }

        public byte[] ToArray()
        {
            //ew
            var tis = TxIn.Sum(a => a.Size);
            var tos = TxOut.Sum(a => a.Size);

            var ret = new byte[8 + tis + tos + TxInCount.Size + TxOutCount.Size];

            var v = BitConverter.GetBytes(Version);
            Array.Copy(v, 0, ret, 0, v.Length);

            var ti = TxInCount.ToArray();
            Array.Copy(ti, 0, ret, 4, ti.Length);

            var txInOffset = 4 + ti.Length;
            for(var x = 0; x < TxInCount; x++)
            {
                var tx = TxIn[x].ToArray();
                Array.Copy(tx, 0, ret, txInOffset, tx.Length);
                txInOffset += tx.Length;
            }

            var to = TxOutCount.ToArray();
            Array.Copy(to, 0, ret, txInOffset, to.Length);

            var txOutOffset = txInOffset + to.Length;
            for(var x = 0; x < TxOutCount; x++)
            {
                var tx = TxOut[x].ToArray();
                Array.Copy(tx, 0, ret, txOutOffset, tx.Length);
                txOutOffset += tx.Length;
            }

            var lt = BitConverter.GetBytes(LockTime);
            Array.Copy(lt, 0, ret, txOutOffset, lt.Length);

            return ret;
        }
    }
}
