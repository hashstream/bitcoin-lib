using hashstream.bitcoin_lib.BlockChain;
using hashstream.bitcoin_lib.P2P;
using System;
using System.Linq;

namespace hashstream.bitcoin_lib.Script
{
    public class WitnessScript : IStreamable
    {
        public VarInt ScriptCount { get; set; }
        public Script[] Stack { get; set; }

        public int Size => ScriptCount.Size + Stack.Sum(a => a.Size);

        public int ReadFromPayload(byte[] data, int offset)
        {
            var roffset = offset;
            ScriptCount = data.ReadFromBuffer<VarInt>(ref roffset);
            
            Stack = new Script[ScriptCount];
            for (var x = 0; x < Stack.Length; x++)
            {
                Stack[x] = data.ReadFromBuffer<Script>(ref roffset);
            }

            return Size;
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];

            var woffset = 0; 
            ret.CopyAndIncr(ScriptCount.ToArray(), ref woffset);
            
            foreach(var s in Stack)
            {
                ret.CopyAndIncr(s.ToArray(), ref woffset);
            }

            return ret;
        }
    }
}
