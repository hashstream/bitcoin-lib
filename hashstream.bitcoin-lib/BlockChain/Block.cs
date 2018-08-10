using hashstream.bitcoin_lib.P2P;
using System;
using System.Linq;

namespace hashstream.bitcoin_lib.BlockChain
{
    public class Block : IStreamable
    {
        public BlockHeader Header { get; set; }
        public VarInt TxnCount { get; set; }
        public Tx[] Txns { get; set; }

        public int Size => BlockHeader.Size + TxnCount.Size + Txns.Sum(a => a.Size);

        public Block() { }

        public Block(BlockHeader h) { Header = h; }

        public int ReadFromPayload(byte[] data, int offset)
        {
            var roffset = offset;
            if (Header == null)
            {
                Header = data.ReadFromBuffer<BlockHeader>(ref roffset);
            }

            TxnCount = data.ReadFromBuffer<VarInt>(ref roffset);

            Txns = new Tx[TxnCount];
            for(var x = 0; x < Txns.Length; x++)
            {
                Txns[x] = data.ReadFromBuffer<Tx>(ref roffset);
            }

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];

            ret.CopyAndIncr(Header.ToArray(), ref woffset);
            ret.CopyAndIncr(TxnCount.ToArray(), ref woffset);

            foreach(var tx in Txns)
            {
                ret.CopyAndIncr(tx.ToArray(), ref woffset);
            }

            return ret;
        }

        public Hash GetBlockHash()
        {
            return Header.GetBlockHeaderHash();
        }
    }
}
