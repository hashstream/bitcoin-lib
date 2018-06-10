using hashstream.bitcoin_lib.P2P;
using System;

namespace hashstream.bitcoin_lib.BlockChain
{
    public class Block : IStreamable
    {
        public BlockHeader Header { get; set; }
        public VarInt TxnCount { get; set; }
        public Tx[] Txns { get; set; }

        public void ReadFromPayload(byte[] data, int offset)
        {
            var roffset = 0;
            Header = new BlockHeader();
            Header.ReadFromPayload(data, roffset);
            roffset += Header.Size;

            TxnCount = new VarInt(0);
            TxnCount.ReadFromPayload(data, roffset);
            roffset += TxnCount.Size;
            
            Txns = new Tx[TxnCount];
            for(var x = 0; x < Txns.Length; x++)
            {
                var txn = new Tx();
                txn.ReadFromPayload(data, roffset);

                Txns[x] = txn;

                roffset += txn.Size;
            }
        }

        public byte[] ToArray()
        {
            throw new NotImplementedException();
        }
    }
}
