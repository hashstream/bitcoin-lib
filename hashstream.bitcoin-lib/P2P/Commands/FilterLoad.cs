using hashstream.bitcoin_lib.BlockChain;
using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class FilterLoad : IStreamable, ICommand
    {
        public VarInt FilterBytes => Filter?.Length;
        public byte[] Filter { get; set; } = new byte[0];
        public UInt32 HashFunctions { get; set; }
        public UInt32 Tweak { get; set; }
        public byte Flags { get; set; }

        public string Command => "filterload";

        public int Size => 9 + FilterBytes + FilterBytes.Size;

#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out VarInt tFilterBytes)
                .ReadAndSlice(tFilterBytes, out byte[] tFilter)
                .ReadAndSlice(out UInt32 tHashFunctions)
                .ReadAndSlice(out UInt32 tTweak)
                .ReadAndSlice(out byte tFlags);
            
            Filter = tFilter;
            HashFunctions = tHashFunctions;
            Tweak = tTweak;
            Flags = tFlags;

            return next;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(FilterBytes)
                .WriteAndSlice(Filter)
                .WriteAndSlice(HashFunctions)
                .WriteAndSlice(Tweak)
                .WriteAndSlice(Flags);
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];
            WriteToPayload(ret);
            return ret;
        }
#else
        public int ReadFromPayload(byte[] data, int offset = 0)
        {
            var roffset = offset;
            var fbc = data.ReadFromBuffer<VarInt>(ref roffset);

            Filter = new byte[fbc];
            Array.Copy(data, roffset + fbc.Size, Filter, 0, Filter.Length);
            roffset += fbc.Size + fbc;

            HashFunctions = data.ReadUInt32FromBuffer(ref roffset);
            Tweak = data.ReadUInt32FromBuffer(ref roffset);
            Flags = data[roffset];

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];

            ret.CopyAndIncr(FilterBytes.ToArray(), ref woffset);
            ret.CopyAndIncr(Filter, ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(HashFunctions), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(Tweak), woffset);

            ret[ret.Length - 1] = Flags;

            return ret;
        }
#endif
    }
}
