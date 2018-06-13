using hashstream.bitcoin_lib.BlockChain;
using hashstream.bitcoin_lib.P2P;
using System;

namespace hashstream.bitcoin_lib.Script
{
    public class WitnessScript : IStreamable
    {
        public VarInt ScriptCount { get; set; }
        public Script[] Stack { get; set; }
        public int TotalLength { get; set; }

        public void ReadFromPayload(byte[] data, int offset)
        {
            ScriptCount = new VarInt(0);
            ScriptCount.ReadFromPayload(data, offset);

            TotalLength = ScriptCount.Size;
            Stack = new Script[ScriptCount];

            var soffset = offset + ScriptCount.Size;
            for (var x = 0; x < Stack.Length; x++)
            {
                var sc = new Script();
                sc.ReadFromPayload(data, soffset);

                Stack[x] = sc;

                soffset += sc.Length.Size + sc.Length;
                TotalLength += sc.Length.Size + sc.Length;
            }
        }

        public byte[] ToArray()
        {
            throw new NotImplementedException();
        }
    }
}
