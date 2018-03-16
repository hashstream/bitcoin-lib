using hashstream.bitcoin_lib.P2P;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hashstream.bitcoin_lib.BlockChain
{
    public class Tx : IStreamable
    {
        public UInt32 Version { get; set; } = 1;
        public VarInt TxInCount { get; set; }
        public TxIn[] TxIn { get; set; }
        public VarInt TxOutCount { get; set; }
        public TxOut[] TxOut { get; set; }
        public UInt32 LockTime { get; set; }
        public int Size { get; private set; }

        public void ReadFromPayload(byte[] data, int offset)
        {
            Size = 8;
            Version = BitConverter.ToUInt32(data, offset);
            TxInCount = new VarInt(0);
            TxInCount.ReadFromPayload(data, offset + 4);

            TxIn = new TxIn[TxInCount];

            int txInOffset = offset + 4 + TxInCount.Size;
            for (var x = 0; x < TxInCount; x++)
            {
                var tx = new TxIn();
                tx.ReadFromPayload(data, txInOffset);

                TxIn[x] = tx;

                txInOffset += tx.Size;
                Size += tx.Size;
            }

            TxOutCount = new VarInt(0);
            TxOutCount.ReadFromPayload(data, txInOffset);

            TxOut = new TxOut[TxOutCount];

            int txOutOffset = txInOffset + TxOutCount.Size;
            for (var x = 0; x < TxOutCount; x++)
            {
                var tx = new TxOut();
                tx.ReadFromPayload(data, txOutOffset);

                TxOut[x] = tx;

                txOutOffset += tx.Size;
                Size += tx.Size;
            }

            LockTime = BitConverter.ToUInt32(data, txOutOffset);
            Size += TxInCount.Size + TxOutCount.Size;
        }

        public byte[] ToArray()
        {
            //ew
            var tis = TxIn.Sum(a => a.Size);
            var tos = TxOut.Sum(a => a.Size);

            var ret = new byte[8 + tis + tos + TxInCount.Size + TxOutCount.Size];

            var v = BitConverter.GetBytes(Version);
            Buffer.BlockCopy(v, 0, ret, 0, v.Length);

            var ti = TxInCount.ToArray();
            Buffer.BlockCopy(ti, 0, ret, 4, ti.Length);

            var txInOffset = 4 + ti.Length;
            for(var x = 0; x < TxInCount; x++)
            {
                var tx = TxIn[x].ToArray();
                Buffer.BlockCopy(tx, 0, ret, txInOffset, tx.Length);
                txInOffset += tx.Length;
            }

            var to = TxOutCount.ToArray();
            Buffer.BlockCopy(to, 0, ret, txInOffset, to.Length);

            var txOutOffset = txInOffset + to.Length;
            for(var x = 0; x < TxOutCount; x++)
            {
                var tx = TxOut[x].ToArray();
                Buffer.BlockCopy(tx, 0, ret, txOutOffset, tx.Length);
                txOutOffset += tx.Length;
            }

            var lt = BitConverter.GetBytes(LockTime);
            Buffer.BlockCopy(lt, 0, ret, txOutOffset, lt.Length);

            return ret;
        }
    }
}
