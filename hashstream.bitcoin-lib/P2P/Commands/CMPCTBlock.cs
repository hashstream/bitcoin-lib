using System;

namespace hashstream.bitcoin_lib.P2P
{
    public class CMPCTBlock : IStreamable, ICommand
    {
        public HeaderAndShortIDs BlockInfo { get; set; }

        public int Size => BlockInfo.Size;

        public string Command => "cmpctblock";


#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out HeaderAndShortIDs tBlockInfo);

            BlockInfo = tBlockInfo;

            return next;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return BlockInfo.WriteToPayload(dest);
        }
#else
        public int ReadFromPayload(byte[] data, int offset = 0)
        {
            var roffset = offset;

            BlockInfo = data.ReadFromBuffer<HeaderAndShortIDs>(ref roffset);

            return Size;
        }
#endif
        
        public byte[] ToArray()
        {
            return BlockInfo.ToArray();
        }
    }
}
