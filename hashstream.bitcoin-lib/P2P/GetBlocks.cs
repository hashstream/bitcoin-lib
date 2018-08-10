using hashstream.bitcoin_lib.BlockChain;
using System;
using System.IO;

namespace hashstream.bitcoin_lib.P2P
{
    public class GetBlocks : IStreamable, ICommand
    {
        public UInt32 Version { get; set; } = Consensus.Version;
        public VarInt HashCount { get; set; }
        public Hash[] Hashes { get; set; }
        public Hash StopHash { get; set; }

        public string Command => "getblocks";

        public int Size => 4 + HashCount.Size + (Hash.Size * HashCount) + Hash.Size;

        public int ReadFromPayload(byte[] data, int offset)
        {
            var roffset = offset;
            Version = data.ReadUInt32FromBuffer(ref roffset);
            HashCount = data.ReadFromBuffer<VarInt>(ref roffset);

            Hashes = new Hash[HashCount];

            for (var x = 0; x < HashCount; x++)
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
    }
}