using hashstream.bitcoin_lib.P2P;
using System;
using System.Linq;

namespace hashstream.bitcoin_lib.BlockChain
{
    public class Block : IStreamable, ICommand
    {
        public BlockHeader Header { get; set; }
        public VarInt TxnCount => Txns?.Length;
        public Tx[] Txns { get; set; } = new Tx[0];

        public int Size => BlockHeader.StaticSize + TxnCount.Size + Txns.Sum(a => a.Size);

        public Hash Hash => Header.Hash;

        public string Command => "block";

        public Block() { }

        public Block(BlockHeader h) { Header = h; }


#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data;
            
            if(Header == null)
            {
                next = next.ReadAndSlice(out BlockHeader bh);
                Header = bh;
            }

            next = next.ReadAndSlice(out VarInt tTxnCount)
                .ReadAndSlice(tTxnCount, out Tx[] tTxns);
            
            Txns = tTxns;

            return next;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(Header)
                .WriteAndSlice(TxnCount)
                .WriteAndSlice(Txns);
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];
            WriteToPayload(ret);
            return ret;
        }
#else
        public int ReadFromPayload(byte[] data, int offset)
        {
            var roffset = offset;

            if (Header == null)
            {
                Header = data.ReadFromBuffer<BlockHeader>(ref roffset);
            }

            var txc = data.ReadFromBuffer<VarInt>(ref roffset);

            Txns = new Tx[txc];
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
#endif
    }
}
