using hashstream.bitcoin_lib.P2P;
using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.BlockChain
{
    public class TxIn : IStreamable
    {
        public Outpoint Previous { get; set; }
        public VarInt ScriptLength { get; set; }
        public byte[] Script { get; set; }
        public UInt32 Sequence { get; set; } = 0xffffffff;

        public int Size => 40 + ScriptLength.Size + ScriptLength;

        public void ReadFromPayload(byte[] data, int offset)
        {
            Previous = new Outpoint();
            Previous.ReadFromPayload(data, offset);

            ScriptLength = new VarInt(0);
            ScriptLength.ReadFromPayload(data, offset + Previous.Size);

            Script = new byte[ScriptLength];
            Buffer.BlockCopy(data, offset + Previous.Size, Script, 0, ScriptLength);

            Sequence = BitConverter.ToUInt32(data, offset + Previous.Size + ScriptLength);
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];

            var p = Previous.ToArray();
            Buffer.BlockCopy(p, 0, ret, 0, p.Length);

            var sl = ScriptLength.ToArray();
            Buffer.BlockCopy(sl, 0, ret, p.Length, sl.Length);

            Buffer.BlockCopy(Script, 0, ret, p.Length + sl.Length, Script.Length);

            var sq = BitConverter.GetBytes(Sequence);
            Buffer.BlockCopy(sq, 0, ret, p.Length + sl.Length + Script.Length, sq.Length);

            return ret;
        }
    }
}
