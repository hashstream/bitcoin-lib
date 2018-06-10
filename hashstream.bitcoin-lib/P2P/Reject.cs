using hashstream.bitcoin_lib.BlockChain;
using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public enum RejectCode
    {
        DecodeError = 0x01,
        InvalidData = 0x10,
        Outdated = 0x11,
        Duplicate = 0x12,
        InvalidTx = 0x40,
        DustyTx = 0x41,
        LowFeeTx = 0x42,
        InvalidChain = 0x43
    }

    public class Reject : IStreamable, ICommand
    {
        public VarInt MessageLength { get; set; }
        public string Message { get; set; }
        public byte Code { get; set; }
        public VarInt ReasonLength { get; set; }
        public string Reason { get; set; }
        public byte[] Extra { get; set; }

        public string Command => "reject";

        public int Size => MessageLength + MessageLength.Size + ReasonLength.Size + ReasonLength + Extra.Length + 1;

        public void ReadFromPayload(byte[] data, int offset)
        {
            MessageLength = new VarInt(0);
            MessageLength.ReadFromPayload(data, offset);

            Message = Encoding.ASCII.GetString(data, offset + MessageLength.Size, MessageLength);
            Code = data[offset + MessageLength + MessageLength.Size];

            ReasonLength = new VarInt(0);
            ReasonLength.ReadFromPayload(data, offset + MessageLength + MessageLength.Size + 1);

            Reason = Encoding.ASCII.GetString(data, offset + MessageLength + MessageLength.Size + 1 + ReasonLength.Size, ReasonLength);
            var of = offset + MessageLength + MessageLength.Size + 1 + ReasonLength.Size + ReasonLength;
            Extra = new byte[data.Length - of]; // ew no length = we have to rely on the length of the buffer (reading more "Extra" than we should)
            Array.Copy(data, of, Extra, 0, Extra.Length);
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];

            var ml = MessageLength.ToArray();
            Array.Copy(ml, 0, ret, 0, ml.Length);

            var mg = Encoding.ASCII.GetBytes(Message);
            Array.Copy(mg, 0, ret, ml.Length, mg.Length);

            ret[mg.Length + ml.Length - 2] = Code;

            var rl = ReasonLength.ToArray();
            Array.Copy(rl, 0, ret, mg.Length + ml.Length + 1, rl.Length);

            var re = Encoding.ASCII.GetBytes(Reason);
            Array.Copy(re, 0, ret, mg.Length + ml.Length + 1 + rl.Length, re.Length);

            Array.Copy(Extra, 0, ret, mg.Length + ml.Length + 1 + rl.Length + re.Length, Extra.Length);

            return ret;
        }
    }
}
