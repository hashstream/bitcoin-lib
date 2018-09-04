using System;

namespace hashstream.bitcoin_lib.P2P
{
    public class GetBlockTxn : IStreamable, ICommand
    {
        public BlockTransactionsRequest Txns { get; set; }

        public int Size => Txns.Size;

        public string Command => "getblocktxn";


#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out BlockTransactionsRequest tTxns);

            Txns = tTxns;

            return next;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return Txns.WriteToPayload(dest);
        }
#else
        public int ReadFromPayload(byte[] data, int offset = 0)
        {
            var roffset = offset;

            Txns = data.ReadFromBuffer<BlockTransactionsRequest>(ref roffset);

            return Size;
        }
#endif
        public byte[] ToArray()
        {
            return Txns.ToArray();
        }
    }
}
