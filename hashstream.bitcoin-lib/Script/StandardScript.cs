using hashstream.bitcoin_lib.BlockChain;
using hashstream.bitcoin_lib.Crypto;
using System;

namespace hashstream.bitcoin_lib.Script
{
    public enum TxOutType
    {
        TX_NONSTANDARD,
        TX_PUBKEY,
        TX_PUBKEYHASH,
        TX_SCRIPTHASH,
        TX_MULTISIG,
        TX_NULL_DATA,
        TX_WITNESS_V0_SCRIPTHASH,
        TX_WITNESS_V0_KEYHASH,
        TX_WITNESS_UNKNOWN
    }

    [Flags]
    public enum SigHashFlags
    {
        SIGHASH_ALL = 1,
        SIGHASH_NONE = 2,
        SIGHASH_SINGLE = 3,
        SIGHASH_ANYONECANPAY = 0x80,
    }

    //https://github.com/bitcoin/bitcoin/blob/master/src/script/interpreter.h#L35
    [Flags]
    public enum ScriptVerificationFlags
    {
        SCRIPT_VERIFY_NONE = 0,
        SCRIPT_VERIFY_P2SH = (1 << 0),
        SCRIPT_VERIFY_STRICTENC = (1 << 1),
        SCRIPT_VERIFY_DERSIG = (1 << 2),
        SCRIPT_VERIFY_LOW_S = (1 << 3),
        SCRIPT_VERIFY_NULLDUMMY = (1 << 4),
        SCRIPT_VERIFY_SIGPUSHONLY = (1 << 5),
        SCRIPT_VERIFY_MINIMALDATA = (1 << 6),
        SCRIPT_VERIFY_DISCOURAGE_UPGRADABLE_NOPS = (1 << 7),
        SCRIPT_VERIFY_CLEANSTACK = (1 << 8),
        SCRIPT_VERIFY_CHECKLOCKTIMEVERIFY = (1 << 9),
        SCRIPT_VERIFY_CHECKSEQUENCEVERIFY = (1 << 10),
        SCRIPT_VERIFY_WITNESS = (1 << 11),
        SCRIPT_VERIFY_DISCOURAGE_UPGRADABLE_WITNESS_PROGRAM = (1 << 12),
        SCRIPT_VERIFY_MINIMALIF = (1 << 13),
        SCRIPT_VERIFY_NULLFAIL = (1 << 14),
        SCRIPT_VERIFY_WITNESS_PUBKEYTYPE = (1 << 15),
        SCRIPT_VERIFY_CONST_SCRIPTCODE = (1 << 16)
    }

    public class StandardScript : Script
    {
        public TxOutType TxType
        {
            get
            {
                if (IsPayToScriptHash())
                {
                    return TxOutType.TX_SCRIPTHASH;
                }
                else if (IsPayToPubKeyHash())
                {
                    return TxOutType.TX_PUBKEYHASH;
                }
                else if (IsPayToPubKey())
                {
                    return TxOutType.TX_PUBKEY;
                }
                else if (IsOpReturn())
                {
                    return TxOutType.TX_NULL_DATA;
                }
                else if (IsPayToWitnessScriptHash())
                {
                    return TxOutType.TX_WITNESS_V0_SCRIPTHASH;
                }
                else if (IsWitnessProgram(out int version))
                {
                    var prog_len = ScriptBytes.Length - 2;
                    if (version == 0 && prog_len == WITNESS_V0_KEYHASH_SIZE)
                    {
                        return TxOutType.TX_WITNESS_V0_KEYHASH;
                    }
                    else if (version == 0 && prog_len == WITNESS_V0_SCRIPTHASH_SIZE)
                    {
                        return TxOutType.TX_WITNESS_V0_SCRIPTHASH;
                    }
                    else if (version != 0)
                    {
                        return TxOutType.TX_WITNESS_UNKNOWN;
                    }
                }
                else if (IsMultiSig())
                {
                    return TxOutType.TX_MULTISIG;
                }

                return TxOutType.TX_NONSTANDARD;
            }
        }

        public string TxTypeString
        {
            get
            {
                switch (TxType)
                {
                    case TxOutType.TX_NONSTANDARD: return "nonstandard";
                    case TxOutType.TX_PUBKEY: return "pubkey";
                    case TxOutType.TX_PUBKEYHASH: return "pubkeyhash";
                    case TxOutType.TX_SCRIPTHASH: return "scripthash";
                    case TxOutType.TX_MULTISIG: return "multisig";
                    case TxOutType.TX_NULL_DATA: return "nulldata";
                    case TxOutType.TX_WITNESS_V0_SCRIPTHASH: return "witness_v0_scripthash";
                    case TxOutType.TX_WITNESS_V0_KEYHASH: return "witness_v0_keyhash";
                    case TxOutType.TX_WITNESS_UNKNOWN: return "witness_unknown";
                }
                return null;
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

        public bool IsOpReturn()
        {
            return ScriptBytes.Length >= 1 && ScriptBytes[0] == (byte)OpCode.OP_RETURN;
        }

        public bool IsPayToPubKey()
        {
            if (ScriptBytes.Length == PubKey.PUBLIC_KEY_SIZE + 2 && ScriptBytes[0] == PubKey.PUBLIC_KEY_SIZE && ScriptBytes[ScriptBytes.Length - 1] == (byte)OpCode.OP_CHECKSIG)
            {
                return true;
            }
            if (ScriptBytes.Length == PubKey.COMPRESSED_PUBLIC_KEY_SIZE && ScriptBytes[0] == PubKey.COMPRESSED_PUBLIC_KEY_SIZE && ScriptBytes[ScriptBytes.Length - 1] == (byte)OpCode.OP_CHECKSIG)
            {
                return true;
            }
            return false;
        }

        public bool IsPayToPubKeyHash()
        {
            if (ScriptBytes.Length == 25 && ScriptBytes[0] == (byte)OpCode.OP_DUP && ScriptBytes[1] == (byte)OpCode.OP_HASH160 && ScriptBytes[2] == 20 && ScriptBytes[23] == (byte)OpCode.OP_EQUALVERIFY && ScriptBytes[24] == (byte)OpCode.OP_CHECKSIG)
            {
                return true;
            }
            return false;
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

        public bool IsMultiSig()
        {
            //m-of-n multisig, dont check keys..
            if(ScriptBytes.Length > 1 && IsSmallInteger((OpCode)ScriptBytes[0]) && IsSmallInteger((OpCode)ScriptBytes[ScriptBytes.Length-2]) && ScriptBytes[ScriptBytes.Length-1] == (byte)OpCode.OP_CHECKMULTISIG)
            {
                return true;
            }
            return false;
        }

        public byte[] GetWitnessProgram()
        {
            var program = new byte[ScriptBytes.Length - 2];
            Array.Copy(ScriptBytes, 2, program, 0, program.Length);

            return program;
        }

        public Address GetAddress()
        {
            var t = TxType;

            if(t == TxOutType.TX_WITNESS_V0_SCRIPTHASH)
            {
                var version = GetSmallIntegerValue(ParsedScript[0]); //first opcode is witness program version
                var script_hash = ParsedScript[1]; //second item is witness program hash
                return new Bech32Address(AddressNetwork.Main, version, script_hash);
            }
            else if(t == TxOutType.TX_WITNESS_V0_KEYHASH)
            {
                var version = GetSmallIntegerValue(ParsedScript[0]); //first opcode is witness program version
                var key_hash = ParsedScript[1]; //second item is witness program hash
                return new Bech32Address(AddressNetwork.Main, version, key_hash);
            }
            else if(t == TxOutType.TX_PUBKEYHASH)
            {
                var key_hash = ParsedScript[2]; //third item is pk hash
                return new Base58Address(AddressNetwork.Main, Base58AddressType.PubKey, key_hash);
            }
            else if(t == TxOutType.TX_SCRIPTHASH)
            {
                var script_hash = ParsedScript[1]; //second item is script hash
                return new Base58Address(AddressNetwork.Main, Base58AddressType.Script, script_hash);
            }
            else if(t == TxOutType.TX_PUBKEY)
            {
                var key = ParsedScript[0]; //first item is the key
                return new Base58Address(AddressNetwork.Main, Base58AddressType.PubKey, ((byte[])key).Hash160());
            }

            return null;
        }
    }
}
