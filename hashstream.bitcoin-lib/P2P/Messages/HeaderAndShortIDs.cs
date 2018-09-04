using hashstream.bitcoin_lib.BlockChain;
using System;
using System.Linq;

namespace hashstream.bitcoin_lib.P2P
{
    public class HeaderAndShortIDs : IStreamable
    {
        public BlockHeader Header { get; set; }
        public UInt64 Nonce { get; set; }
        public VarInt ShortIdsCount => ShortIds?.Length;
        public UInt48[] ShortIds { get; set; }
        public VarInt PrefilledTransactionsCount => PrefilledTransactions?.Length;
        public PrefilledTransaction[] PrefilledTransactions { get; set; }

        public int Size => Header.Size + 8 + ShortIds.Sum(a => a.Size) + ShortIdsCount.Size + PrefilledTransactions.Sum(a => a.Size) + PrefilledTransactionsCount.Size;
        
#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out BlockHeader tHeader)
                .ReadAndSlice(out UInt64 tNonce)
                .ReadAndSlice(out VarInt tShortIdsCount)
                .ReadAndSlice(tShortIdsCount, out UInt48[] tShortIds)
                .ReadAndSlice(out VarInt tPrefilledTransactionsCount)
                .ReadAndSlice(tPrefilledTransactionsCount, out PrefilledTransaction[] tPrefilledTransactions);

            Header = tHeader;
            Nonce = tNonce;
            ShortIds = tShortIds;
            PrefilledTransactions = tPrefilledTransactions;

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
            return dest.WriteAndSlice(Header)
                .WriteAndSlice(Nonce)
                .WriteAndSlice(ShortIdsCount)
                .WriteAndSlice(ShortIds)
                .WriteAndSlice(PrefilledTransactionsCount)
                .WriteAndSlice(PrefilledTransactions);
        }
#else
        public int ReadFromPayload(byte[] data, int offset = 0)
        {
            var roffset = offset;

            Header = data.ReadFromBuffer<BlockHeader>(ref roffset);
            Nonce = data.ReadUInt64FromBuffer(ref roffset);

            var tShortIds = data.ReadFromBuffer<VarInt>(ref roffset);
            ShortIds = data.ReadFromBuffer<UInt48>(tShortIds, ref roffset);

            var tPrefilledTransactionsCount = data.ReadFromBuffer<VarInt>(ref roffset);
            PrefilledTransactions = data.ReadFromBuffer<PrefilledTransaction>(tPrefilledTransactionsCount, ref roffset);

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];

            ret.CopyAndIncr(Header, ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(Nonce), ref woffset);
            ret.CopyAndIncr(ShortIdsCount, ref woffset);
            ret.CopyAndIncr(ShortIds, ref woffset);
            ret.CopyAndIncr(PrefilledTransactionsCount, ref woffset);
            ret.CopyAndIncr(PrefilledTransactions, ref woffset);

            return ret;
        }
#endif
    }
}
