using hashstream.bitcoin_lib.P2P;
using System;

namespace hashstream.bitcoin_lib.BlockChain
{
    public class TxOut : IStreamable
    {
        public UInt64 Value { get; set; }
        public VarInt ScriptLength { get; set; }
        public byte[] Script { get; set; }

        public int Size { get; private set; }

        public void ReadFromPayload(byte[] data, int offset)
        {
            Value = BitConverter.ToUInt64(data, offset);
            ScriptLength = new VarInt(0);
            ScriptLength.ReadFromPayload(data, offset + 8);

            Script = new byte[ScriptLength];
            Array.Copy(data, offset + 8 + ScriptLength.Size, Script, 0, Script.Length);

            Size = 8 + ScriptLength + ScriptLength.Size;
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];

            var v = BitConverter.GetBytes(Value);
            Array.Copy(v, 0, ret, 0, v.Length);

            var sl = ScriptLength.ToArray();
            Array.Copy(sl, 0, ret, 8, sl.Length);

            Array.Copy(Script, 0, ret, 8 + sl.Length, Script.Length);

            return ret;
        }
    }
}
