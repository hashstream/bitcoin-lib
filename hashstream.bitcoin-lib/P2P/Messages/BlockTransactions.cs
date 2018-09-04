using hashstream.bitcoin_lib.BlockChain;
using System;

namespace hashstream.bitcoin_lib.P2P
{
    public class BlockTransactions : IStreamable
    {
        public Hash BlockHash { get; set; }
        public VarInt TransactionsCount => Transactions?.Length;
        public Tx[] Transactions { get; set; }

        public int Size => throw new NotImplementedException();


#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out Hash tBlockHash)
                .ReadAndSlice(out VarInt tTransactionsCount)
                .ReadAndSlice(tTransactionsCount, out Tx[] tTransactions);

            BlockHash = tBlockHash;
            Transactions = tTransactions;

            return next;
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];
            WriteToPayload(ret);
            return ret;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(BlockHash)
                .WriteAndSlice(TransactionsCount)
                .WriteAndSlice(Transactions);
        }
#else
        public int ReadFromPayload(byte[] data, int offset = 0)
        {
            var roffset = offset;

            BlockHash = data.ReadFromBuffer<Hash>(ref roffset);

            var tTransactionCount = data.ReadFromBuffer<VarInt>(ref roffset);
            Transactions = data.ReadFromBuffer<Tx>(tTransactionCount, ref roffset);

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];

            ret.CopyAndIncr(BlockHash, ref woffset);
            ret.CopyAndIncr(TransactionsCount, ref woffset);
            ret.CopyAndIncr(Transactions, ref woffset);

            return ret;
        }
#endif
    }
}
