using hashstream.bitcoin_lib.BlockChain;
using hashstream.bitcoin_lib.P2P;
using System;
using System.Linq;

namespace hashstream.bitcoin_lib.Script
{
    public class WitnessScript : IStreamable
    {
        public VarInt ScriptCount => Stack?.Length;
        public Script[] Stack { get; set; } = new Script[0];

        public int Size => ScriptCount.Size + Stack.Sum(a => a.Size);
        
#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out VarInt tScriptCount)
                .ReadAndSlice(tScriptCount, out Script[] tStack);

            Stack = tStack;

            return next;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(ScriptCount)
                .WriteAndSlice(Stack);
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
            var sc = data.ReadFromBuffer<VarInt>(ref roffset);
            
            Stack = new Script[sc];
            for (var x = 0; x < sc; x++)
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
#endif
    }
}
