using System;

namespace hashstream.bitcoin_lib.P2P
{
    public class Ping : IStreamable, ICommand
    {
        public UInt64 Nonce { get; set; }

        public string Command => "ping";

        public int Size => StaticSize;

        public static int StaticSize => 8;

        public Ping()
        {
            var nonce = new byte[8];
            new Random().NextBytes(nonce);
            Nonce = BitConverter.ToUInt64(nonce, 0);
        }

#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out UInt64 tNonce);

            Nonce = tNonce;

            return next;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(Nonce);
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
            Nonce = BitConverter.ToUInt64(data, offset);

            return Size;
        }

        public byte[] ToArray()
        {
            return BitConverter.GetBytes(Nonce);
        }
#endif
    }
}
