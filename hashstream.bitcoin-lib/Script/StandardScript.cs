using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.Script
{
    public enum TxOutType
    {
        TX_NONSTANDARD,
        // 'standard' transaction types:
        TX_PUBKEY,
        TX_PUBKEYHASH,
        TX_SCRIPTHASH,
        TX_MULTISIG,
        TX_NULL_DATA, //!< unspendable OP_RETURN script that carries data
        TX_WITNESS_V0_SCRIPTHASH,
        TX_WITNESS_V0_KEYHASH,
        TX_WITNESS_UNKNOWN, //!< Only for Witness versions not already defined above
    }

    [Flags]
    public enum SigHashFlags
    {
        SIGHASH_ALL = 1,
        SIGHASH_NONE = 2,
        SIGHASH_SINGLE = 3,
        SIGHASH_ANYONECANPAY = 0x80,
    }

    public class StandardScript : Script
    {
        public TxOutType Type
        {
            get
            {
                if (IsPayToScriptHash())
                {
                    return TxOutType.TX_SCRIPTHASH;
                }

                return TxOutType.TX_NONSTANDARD;
            }
        }

        public new void ReadFromPayload(byte[] data, int offset)
        {
            base.ReadFromPayload(data, offset);
            ParsedScript = ParseScript(ScriptBytes);
        }

        public bool IsPayToScriptHash()
        {
            return ScriptBytes.Length == 23 &&
                ScriptBytes[0] == (byte)OpCode.OP_HASH160 &&
                ScriptBytes[1] == 0x14 &&
                ScriptBytes[22] == (byte)OpCode.OP_EQUAL;
        }

        public bool IsPayToWitnessScriptHash()
        {
            return ScriptBytes.Length == 34 &&
                ScriptBytes[0] == (byte)OpCode.OP_0 &&
                ScriptBytes[1] == 0x20;
        }

        public bool IsWitnessProgram(out int version)
        {
            version = -1;
            if (ScriptBytes.Length < 4 || ScriptBytes.Length > 42)
            {
                return false;
            }

            if (ScriptBytes[0] != (byte)OpCode.OP_0 && (ScriptBytes[0] < (byte)OpCode.OP_1 || ScriptBytes[0] > (byte)OpCode.OP_16))
            {
                return false;
            }

            if (ScriptBytes[1] + 2 == ScriptBytes.Length)
            {
                version = ScriptBytes[0];
                return true;
            }

            return false;
        }
    }
}
