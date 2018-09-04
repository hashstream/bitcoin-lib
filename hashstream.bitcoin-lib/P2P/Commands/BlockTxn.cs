using System;

namespace hashstream.bitcoin_lib.P2P
{
    public class BlockTxn : IStreamable, ICommand
    {
        public BlockTransactions Txns { get; set; }

        public int Size => Txns.Size;

        public string Command => "blocktxn";


#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out BlockTransactions tTxns);

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

            Txns = data.ReadFromBuffer<BlockTransactions>(ref roffset);

            return Size;
        }
#endif
        public byte[] ToArray()
        {
            return Txns.ToArray();
        }
    }
}
