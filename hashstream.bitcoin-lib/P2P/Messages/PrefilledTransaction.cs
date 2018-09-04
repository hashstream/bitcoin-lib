using hashstream.bitcoin_lib.BlockChain;
using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class PrefilledTransaction : IStreamable
    {
        public VarInt Index { get; set; }
        public Tx Transaction { get; set; }

        public int Size => Index.Size + Transaction.Size;

#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out VarInt tIndex)
                .ReadAndSlice(out Tx tTransaction);

            Index = tIndex;
            Transaction = tTransaction;

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
            return dest.WriteAndSlice(Index)
                .WriteAndSlice(Transaction);
        }
#else
        public int ReadFromPayload(byte[] data, int offset = 0)
        {
            var roffset = offset;

            Index = data.ReadFromBuffer<VarInt>(ref roffset);
            Transaction = data.ReadFromBuffer<Tx>(ref roffset);

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];

            ret.CopyAndIncr(Index.ToArray(), ref woffset);
            ret.CopyAndIncr(Transaction.ToArray(), woffset);

            return ret;
        }
#endif
    }
}
