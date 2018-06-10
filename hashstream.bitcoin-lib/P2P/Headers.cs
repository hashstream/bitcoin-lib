using hashstream.bitcoin_lib.BlockChain;
using System;

namespace hashstream.bitcoin_lib.P2P
{
    public class Headers : IStreamable, ICommand
    {
        public VarInt Count { get; set; }
        public BlockHeader[] Header { get; set; }

        public string Command => "headers";

        public void ReadFromPayload(byte[] data, int offset)
        {
            Count = new VarInt(0);
            Count.ReadFromPayload(data, offset);
            Header = new BlockHeader[Count];

            for (var x = 0; x < Count; x++)
            {
                var bh = new BlockHeader();
                bh.ReadFromPayload(data, offset + Count.Size + (80 * x));

                Header[x] = bh;
            }
        }

        public byte[] ToArray()
        {
            var ret = new byte[Count.Size + (Count * 80)];

            var c = Count.ToArray();
            Array.Copy(c, 0, ret, 0, c.Length);

            for (var x = 0; x < Count; x++)
            {
                var bh = Header[x];
                var dt = bh.ToArray();
                Array.Copy(dt, 0, ret, Count.Size + (x * 80), dt.Length);
            }

            return ret;
        }
    }
}
