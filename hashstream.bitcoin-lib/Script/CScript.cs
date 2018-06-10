using hashstream.bitcoin_lib.BlockChain;
using hashstream.bitcoin_lib.P2P;
using System;

namespace hashstream.bitcoin_lib.Script
{
    public class CScript : IStreamable
    {
        public VarInt Length { get; set; }
        public byte[] Stack { get; set; }

        public void ReadFromPayload(byte[] data, int offset)
        {
            Length = new VarInt(0);
            Length.ReadFromPayload(data, offset);

            Stack = new byte[Length];
            Buffer.BlockCopy(data, offset + Length.Size, Stack, 0, Stack.Length);
        }

        public byte[] ToArray()
        {
            throw new NotImplementedException();
        }
    }
}
