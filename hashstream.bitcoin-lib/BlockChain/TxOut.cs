using hashstream.bitcoin_lib.P2P;
using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.BlockChain
{
    public class TxOut : IStreamable
    {
        public UInt64 Value { get; set; }
        public VarInt ScriptLength { get; set; }
        public byte[] Script { get; set; }

        public int Size => 8 + ScriptLength + ScriptLength.Size;

        public void ReadFromPayload(byte[] data, int offset)
        {
            Value = BitConverter.ToUInt64(data, offset);
            ScriptLength = new VarInt(0);
            ScriptLength.ReadFromPayload(data, offset + 8);

            Script = new byte[ScriptLength];
            Buffer.BlockCopy(data, offset + 8 + ScriptLength.Size, Script, 0, Script.Length);
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];

            var v = BitConverter.GetBytes(Value);
            Buffer.BlockCopy(v, 0, ret, 0, v.Length);

            var sl = ScriptLength.ToArray();
            Buffer.BlockCopy(sl, 0, ret, 8, sl.Length);

            Buffer.BlockCopy(Script, 0, ret, 8 + sl.Length, Script.Length);

            return ret;
        }
    }
}
