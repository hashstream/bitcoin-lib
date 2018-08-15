using hashstream.bitcoin_lib.BlockChain;
using System;
using System.IO;

namespace hashstream.bitcoin_lib.P2P
{
    public class GetBlocks : IStreamable, ICommand
    {
        public UInt32 Version { get; set; } = Consensus.Version;
        public VarInt HashCount => Hashes?.Length;
        public Hash[] Hashes { get; set; } = new Hash[0];
        public Hash StopHash { get; set; }

        public string Command => "getblocks";

        public int Size => 4 + HashCount.Size + (Hash.StaticSize * HashCount) + Hash.StaticSize;

#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out UInt32 tVersion)
                .ReadAndSlice(out VarInt tHashCount)
                .ReadAndSlice(tHashCount, out Hash[] tHashes)
                .ReadAndSlice(out Hash tStopHash);

            Version = tVersion;
            Hashes = tHashes;
            StopHash = tStopHash;

            return next;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(Version)
                .WriteAndSlice(HashCount)
                .WriteAndSlice(Hashes)
                .WriteAndSlice(StopHash);
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
            Version = data.ReadUInt32FromBuffer(ref roffset);
            var hc = data.ReadFromBuffer<VarInt>(ref roffset);

            Hashes = new Hash[hc];
            for (var x = 0; x < hc; x++)
            {
                Hashes[x] = data.ReadFromBuffer<Hash>(ref roffset);
            }

            StopHash = data.ReadFromBuffer<Hash>(ref roffset);

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];

            ret.CopyAndIncr(BitConverter.GetBytes(Version), ref woffset);
            ret.CopyAndIncr(HashCount.ToArray(), ref woffset);

            foreach(var hash in Hashes)
            {
                ret.CopyAndIncr(hash.ToArray(), ref woffset);
            }

            ret.CopyAndIncr(StopHash.ToArray(), woffset);

            return ret;
        }
#endif
    }
}