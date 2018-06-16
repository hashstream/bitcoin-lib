using hashstream.bitcoin_lib.BlockChain;
using hashstream.bitcoin_lib.Crypto;
using hashstream.bitcoin_lib.P2P;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace hashstream.bitcoin_lib.Script
{
    public class Script : IStreamable
    {
        public static uint MAX_SCRIPT_ELEMENT_SIZE => 520;
        public static int MAX_OPS_PER_SCRIPT => 201;
        public static int MAX_PUBKEYS_PER_MULTISIG => 20;
        public static int MAX_SCRIPT_SIZE => 10000;
        public static int MAX_STACK_SIZE => 1000;
        public static int WITNESS_V0_SCRIPTHASH_SIZE => 32;
        public static int WITNESS_V0_KEYHASH_SIZE => 20;

        public VarInt Length { get; internal set; }
        public byte[] ScriptBytes { get; internal set; }
        public ScriptFrame[] ParsedScript { get; internal set; }
        public int Size => Length + Length.Size;

        public Script() { }

        public Script(byte[] data, bool parse = true)
        {
            ScriptBytes = data;
            Length = new VarInt((ulong)data.LongLength);

            if (parse)
            {
                ParsedScript = ParseScript(ScriptBytes);
            }
        }

        public void ReadFromPayload(byte[] data, int offset)
        {
            Length = new VarInt(0);
            Length.ReadFromPayload(data, offset);

            ScriptBytes = new byte[Length];
            Array.Copy(data, offset + Length.Size, ScriptBytes, 0, ScriptBytes.Length);
        }

        public byte[] ToArray()
        {
            var ret = new byte[Length.Size + Length];
            Array.Copy(Length.ToArray(), ret, Length.Size);
            Array.Copy(ScriptBytes, 0, ret, Length.Size, ScriptBytes.Length);

            return ret;
        }

        public bool ValidateRedeemScript(Script scriptSig)
        {
            var script = new List<ScriptFrame>();
            script.AddRange(scriptSig.ParsedScript);
            script.AddRange(ParsedScript);

            var tf = ValidateParsedScript(script.ToArray());
            return tf.Number == 1;
        }

        /// <summary>
        /// Runs a parsed script and returns the top stack value
        /// </summary>
        /// <param name="script">The parsed script to run</param>
        /// <returns>Top stack value</returns>
        public ScriptFrame ValidateParsedScript(ScriptFrame[] script)
        {
            var stack = new Stack<ScriptFrame>();
            var alt_stack = new Stack<ScriptFrame>();

            for (var x = 0; x < script.Length; x++)
            {
                var fx = script[x];

                if (!fx.IsOp)
                {
                    stack.Push(fx);
                }
                else
                {
                    switch (fx.Op)
                    {
                        // push constants
                        case OpCode.OP_1NEGATE: stack.Push(-1); break;
                        case OpCode.OP_1: stack.Push(1); break;
                        case OpCode.OP_2: stack.Push(2); break;
                        case OpCode.OP_3: stack.Push(3); break;
                        case OpCode.OP_4: stack.Push(4); break;
                        case OpCode.OP_5: stack.Push(5); break;
                        case OpCode.OP_6: stack.Push(6); break;
                        case OpCode.OP_7: stack.Push(7); break;
                        case OpCode.OP_8: stack.Push(8); break;
                        case OpCode.OP_9: stack.Push(9); break;
                        case OpCode.OP_10: stack.Push(10); break;
                        case OpCode.OP_11: stack.Push(11); break;
                        case OpCode.OP_12: stack.Push(12); break;
                        case OpCode.OP_13: stack.Push(13); break;
                        case OpCode.OP_14: stack.Push(14); break;
                        case OpCode.OP_15: stack.Push(15); break;
                        case OpCode.OP_16: stack.Push(16); break;

                        //flow control
                        //TODO: this..
                        case OpCode.OP_NOP: break;
                        case OpCode.OP_ENDIF: break;
                        case OpCode.OP_IF:
                        case OpCode.OP_NOTIF:
                        case OpCode.OP_ELSE:
                            {
                                //search script for endif
                                for (var z = x; z < script.Length; z++)
                                {
                                    if (script[z].IsOp && script[x].Op == OpCode.OP_ENDIF)
                                    {
                                        x = z;
                                        break;
                                    }
                                }
                                break;
                            }
                        case OpCode.OP_VERIFY:
                            {
                                if (!stack.Pop())
                                {
                                    return 0;
                                }
                                break;
                            }

                        //stack ops
                        case OpCode.OP_TOALTSTACK: alt_stack.Push(stack.Pop()); break;
                        case OpCode.OP_FROMALTSTACK: stack.Push(alt_stack.Pop()); break;
                        case OpCode.OP_DROP: stack.Pop(); break;
                        case OpCode.OP_2DROP: stack.Pop(); stack.Pop(); break;
                        case OpCode.OP_DUP: stack.Push(stack.Peek()); break;
                        case OpCode.OP_IFDUP:
                            {
                                var eval = stack.Pop();
                                if (eval.IsNumber && eval.Number != 0)
                                {
                                    stack.Push(eval);
                                    stack.Push(eval);
                                }
                                break;
                            }
                        case OpCode.OP_NIP:
                            {
                                var a = stack.Pop();
                                stack.Pop();
                                stack.Push(a);
                                break;
                            }
                        case OpCode.OP_OVER:
                            {
                                var a = stack.Pop();
                                var b = stack.Pop();
                                stack.Push(a);
                                stack.Push(b);
                                stack.Push(a);
                                break;
                            }
                        case OpCode.OP_PICK:
                            {
                                var z = stack.Pop();
                                var zstack = new Stack<ScriptFrame>();
                                for (var y = 1; y < z.Number; y++)
                                {
                                    zstack.Push(stack.Pop());
                                }

                                var n = stack.Peek();
                                foreach (var s in zstack)
                                {
                                    stack.Push(s);
                                }
                                stack.Push(n);
                                break;
                            }
                        case OpCode.OP_ROLL:
                            {
                                var z = stack.Pop();
                                var zstack = new Stack<ScriptFrame>();
                                for (var y = 1; y < z.Number; y++)
                                {
                                    zstack.Push(stack.Pop());
                                }

                                var n = stack.Pop();
                                foreach (var s in zstack)
                                {
                                    stack.Push(s);
                                }
                                stack.Push(n);
                                break;
                            }
                        case OpCode.OP_ROT:
                            {
                                var a = stack.Pop();
                                var b = stack.Pop();
                                var c = stack.Pop();

                                stack.Push(b);
                                stack.Push(a);
                                stack.Push(c);
                                break;
                            }
                        case OpCode.OP_SWAP:
                            {
                                var a = stack.Pop();
                                var b = stack.Pop();
                                stack.Push(a);
                                stack.Push(b);
                                break;
                            }
                        case OpCode.OP_TUCK:
                            {
                                var a = stack.Pop();
                                var b = stack.Pop();
                                stack.Push(a);
                                stack.Push(b);
                                stack.Push(a);
                                break;
                            }
                        case OpCode.OP_2DUP:
                            {
                                var a = stack.Pop();
                                var b = stack.Pop();
                                stack.Push(b);
                                stack.Push(a);
                                stack.Push(b);
                                stack.Push(a);
                                break;
                            }
                        case OpCode.OP_3DUP:
                            {
                                var a = stack.Pop();
                                var b = stack.Pop();
                                var c = stack.Pop();
                                stack.Push(c);
                                stack.Push(b);
                                stack.Push(a);
                                stack.Push(c);
                                stack.Push(b);
                                stack.Push(a);
                                break;
                            }
                        case OpCode.OP_2OVER:
                            {
                                var a = stack.Pop();
                                var b = stack.Pop();
                                var c = stack.Pop();
                                var d = stack.Pop();
                                stack.Push(d);
                                stack.Push(c);
                                stack.Push(b);
                                stack.Push(a);
                                stack.Push(d);
                                stack.Push(c);
                                break;
                            }
                        case OpCode.OP_2ROT:
                            {
                                var a = stack.Pop();
                                var b = stack.Pop();
                                var c = stack.Pop();
                                var d = stack.Pop();
                                var e = stack.Pop();
                                var f = stack.Pop();
                                stack.Push(d);
                                stack.Push(c);
                                stack.Push(b);
                                stack.Push(a);
                                stack.Push(f);
                                stack.Push(e);
                                break;
                            }
                        case OpCode.OP_2SWAP:
                            {
                                var a = stack.Pop();
                                var b = stack.Pop();
                                var c = stack.Pop();
                                var d = stack.Pop();
                                stack.Push(b);
                                stack.Push(a);
                                stack.Push(d);
                                stack.Push(c);
                                break;
                            }

                        //splice
                        case OpCode.OP_CAT:
                        case OpCode.OP_SUBSTR:
                        case OpCode.OP_LEFT:
                        case OpCode.OP_RIGHT: throw new Exception($"Disabled script op {fx.Op.ToString()}");
                        case OpCode.OP_SIZE: { var s = stack.Peek(); stack.Push(s.Length); break; }

                        //bitwise logic
                        case OpCode.OP_INVERT:
                        case OpCode.OP_AND:
                        case OpCode.OP_OR:
                        case OpCode.OP_XOR: throw new Exception($"Disabled script op {fx.Op.ToString()}");
                        case OpCode.OP_EQUAL:
                            {
                                var a = stack.Pop();
                                var b = stack.Pop();

                                stack.Push(a.Equals(b));
                                break;
                            }
                        case OpCode.OP_EQUALVERIFY:
                            {
                                var a = stack.Pop();
                                var b = stack.Pop();

                                if (a == b)
                                {
                                    return 0;
                                }
                                break;
                            }

                        //fast maths
                        case OpCode.OP_2MUL:
                        case OpCode.OP_2DIV:
                        case OpCode.OP_MUL:
                        case OpCode.OP_DIV:
                        case OpCode.OP_MOD:
                        case OpCode.OP_LSHIFT:
                        case OpCode.OP_RSHIFT: throw new Exception($"Disabled script op {fx.Op.ToString()}");
                        case OpCode.OP_1ADD: stack.Push(stack.Pop() + 1); break;
                        case OpCode.OP_1SUB: stack.Push(stack.Pop() - 1); break;
                        case OpCode.OP_NEGATE: stack.Push(-stack.Pop()); break;
                        case OpCode.OP_ABS: stack.Push(Math.Abs(stack.Pop())); break;
                        case OpCode.OP_NOT: stack.Push(stack.Pop() == 0); break;
                        case OpCode.OP_0NOTEQUAL: stack.Push(stack.Pop() == 0 ? 0 : 1); break;
                        case OpCode.OP_ADD:
                            {
                                var a = stack.Pop();
                                var b = stack.Pop();
                                stack.Push(a + b);
                                break;
                            }
                        case OpCode.OP_SUB:
                            {
                                var b = stack.Pop();
                                var a = stack.Pop();
                                stack.Push(a - b);
                                break;
                            }
                        case OpCode.OP_BOOLAND:
                            {
                                var a = stack.Pop();
                                var b = stack.Pop();
                                stack.Push(a.Length != 0 && b.Length != 0);
                                break;
                            }
                        case OpCode.OP_BOOLOR:
                            {
                                var a = stack.Pop();
                                var b = stack.Pop();
                                stack.Push(a.Length != 0 || b.Length != 0);
                                break;
                            }
                        case OpCode.OP_NUMEQUAL:
                            {
                                var a = stack.Pop();
                                var b = stack.Pop();

                                stack.Push(a.IsNumber && b.IsNumber && a.Equals(b));
                                break;
                            }
                        case OpCode.OP_NUMEQUALVERIFY:
                            {
                                var a = stack.Pop();
                                var b = stack.Pop();

                                if (!a.IsNumber && b.IsNumber && a.Equals(b))
                                {
                                    return 0;
                                }
                                break;
                            }
                        case OpCode.OP_NUMNOTEQUAL:
                            {
                                var a = stack.Pop();
                                var b = stack.Pop();

                                stack.Push(a != b);
                                break;
                            }
                        case OpCode.OP_LESSTHAN:
                            {
                                var b = stack.Pop();
                                var a = stack.Pop();

                                stack.Push(a < b);
                                break;
                            }
                        case OpCode.OP_GREATERTHAN:
                            {
                                var b = stack.Pop();
                                var a = stack.Pop();

                                stack.Push(a > b);
                                break;
                            }
                        case OpCode.OP_LESSTHANOREQUAL:
                            {
                                var b = stack.Pop();
                                var a = stack.Pop();

                                stack.Push(a <= b);
                                break;
                            }
                        case OpCode.OP_GREATERTHANOREQUAL:
                            {
                                var b = stack.Pop();
                                var a = stack.Pop();

                                stack.Push(a >= b);
                                break;
                            }
                        case OpCode.OP_MIN:
                            {
                                var a = stack.Pop();
                                var b = stack.Pop();

                                stack.Push(Math.Min(a, b));
                                break;
                            }
                        case OpCode.OP_MAX:
                            {
                                var a = stack.Pop();
                                var b = stack.Pop();

                                stack.Push(Math.Max(a, b));
                                break;
                            }
                        case OpCode.OP_WITHIN:
                            {
                                var a = stack.Pop();
                                var min = stack.Pop();
                                var max = stack.Pop();

                                stack.Push(a >= min && a < max);
                                break;
                            }

                        //crypto
                        case OpCode.OP_CODESEPARATOR: break;
                        case OpCode.OP_RIPEMD160:
                            {
                                var data = stack.Pop();
                                using (var cx = new RIPEMD160Managed())
                                {
                                    var hash = cx.ComputeHash(data);
                                    stack.Push(hash);
                                }
                                break;
                            }
                        case OpCode.OP_SHA1:
                            {
                                var data = stack.Pop();
                                using (var cx = new SHA1Managed())
                                {
                                    var hash = cx.ComputeHash(data);
                                    stack.Push(hash);
                                }
                                break;
                            }
                        case OpCode.OP_SHA256:
                            {
                                var data = stack.Pop();
                                using (var cx = new SHA256Managed())
                                {
                                    var hash = cx.ComputeHash(data);
                                    stack.Push(hash);
                                }
                                break;
                            }
                        case OpCode.OP_HASH160:
                            {
                                var data = stack.Pop();
                                using (var cx = new SHA256Managed())
                                {
                                    var hash = cx.ComputeHash(data);
                                    using (var cx2 = new RIPEMD160Managed())
                                    {
                                        var hash2 = cx.ComputeHash(hash);
                                        stack.Push(hash2);
                                    }
                                }
                                break;
                            }
                        case OpCode.OP_HASH256:
                            {
                                var data = stack.Pop();
                                using (var cx = new SHA256Managed())
                                {
                                    var hash = cx.ComputeHash(data);
                                    cx.Clear();
                                    hash = cx.ComputeHash(hash);
                                    stack.Push(hash);
                                }
                                break;
                            }
                        case OpCode.OP_CHECKSIGVERIFY:
                        case OpCode.OP_CHECKSIG:
                            {
                                var pubkey = stack.Pop();
                                var sig = stack.Pop();

                                //hmm needs external lib... secp256k1 not implemented in .NET core
                                stack.Push(1);

                                if (fx.Op == OpCode.OP_CHECKSIGVERIFY)
                                {
                                    if (stack.Pop())
                                    {
                                        return 1;
                                    }
                                }

                                throw new NotImplementedException();
                                break;
                            }
                        case OpCode.OP_CHECKMULTISIGVERIFY:
                        case OpCode.OP_CHECKMULTISIG:
                            {
                                var nsig = stack.Pop();
                                var pubkeys = new ScriptFrame[nsig];
                                var sigs = new ScriptFrame[nsig];

                                //load keys
                                for (var z = nsig; z > 0; z--)
                                {
                                    pubkeys[z] = stack.Pop();
                                }
                                //load sigs
                                for (var z = nsig; z > 0; z--)
                                {
                                    sigs[z] = stack.Pop();
                                }

                                //how many sigs to verify
                                var nverify = stack.Pop();

                                //cry because no secp256k1 implementation
                                stack.Push(1);

                                if (fx.Op == OpCode.OP_CHECKMULTISIGVERIFY)
                                {
                                    if (stack.Pop())
                                    {
                                        return 1;
                                    }
                                }

                                throw new NotImplementedException();
                                break;
                            }
                    }
                }
            }

            return stack.Pop();
        }

        public static ScriptFrame[] ParseScript(byte[] data)
        {
            var ret = new List<ScriptFrame>();

            if (data.Length > 0)
            {
                for (uint x = 0; x < data.Length; x++)
                {
                    var op = (OpCode)data[x];

                    if (op > OpCode.OP_0 && op <= OpCode.OP_PUSHDATA4)
                    {
                        uint nSize = 0;
                        uint start = 0;
                        if (op < OpCode.OP_PUSHDATA1)
                        {
                            nSize = (uint)op;
                            start = x + 1;
                        }
                        else
                        {
                            ret.Add(new ScriptFrame(op));
                            if (op == OpCode.OP_PUSHDATA1)
                            {
                                nSize = data[x + 1];
                                start = x + 2;
                            }
                            else if (op == OpCode.OP_PUSHDATA2)
                            {
                                nSize = BitConverter.ToUInt16(data, (int)x + 1);
                                start = x + 3;
                            }
                            else if (op == OpCode.OP_PUSHDATA4)
                            {
                                nSize = BitConverter.ToUInt32(data, (int)x + 1);
                                start = x + 5;
                            }
                        }
                        
                        if (start + nSize > data.Length)
                        {
                            ret.Add(new ScriptFrame(-1));
                            break;
                        }
                        var hdata = new byte[nSize];
                        Array.Copy(data, start, hdata, 0, hdata.Length);

                        ret.Add(new ScriptFrame(hdata));
                        x = start + nSize - 1;
                    }
                    else
                    {
                        ret.Add(new ScriptFrame(op));
                    }
                }
            }

            return ret.ToArray();
        }

        public override string ToString()
        {
            if (ParsedScript != null && ParsedScript.Length > 0)
            {
                return string.Join<ScriptFrame>(" ", ParsedScript);
            }
            else
            {
                return BitConverter.ToString(ScriptBytes).Replace("-", "").ToLower();
            }
        }
    }

    //https://github.com/bitcoin/bitcoin/blob/master/src/script/script.h#L48
    public enum OpCode
    {
        OP_0 = 0x00,
        OP_FALSE = OP_0,
        OP_PUSHDATA1 = 0x4c,
        OP_PUSHDATA2 = 0x4d,
        OP_PUSHDATA4 = 0x4e,
        OP_1NEGATE = 0x4f,
        OP_RESERVED = 0x50,
        OP_1 = 0x51,
        OP_TRUE = OP_1,
        OP_2 = 0x52,
        OP_3 = 0x53,
        OP_4 = 0x54,
        OP_5 = 0x55,
        OP_6 = 0x56,
        OP_7 = 0x57,
        OP_8 = 0x58,
        OP_9 = 0x59,
        OP_10 = 0x5a,
        OP_11 = 0x5b,
        OP_12 = 0x5c,
        OP_13 = 0x5d,
        OP_14 = 0x5e,
        OP_15 = 0x5f,
        OP_16 = 0x60,

        // control
        OP_NOP = 0x61,
        OP_VER = 0x62,
        OP_IF = 0x63,
        OP_NOTIF = 0x64,
        OP_VERIF = 0x65,
        OP_VERNOTIF = 0x66,
        OP_ELSE = 0x67,
        OP_ENDIF = 0x68,
        OP_VERIFY = 0x69,
        OP_RETURN = 0x6a,

        // stack ops
        OP_TOALTSTACK = 0x6b,
        OP_FROMALTSTACK = 0x6c,
        OP_2DROP = 0x6d,
        OP_2DUP = 0x6e,
        OP_3DUP = 0x6f,
        OP_2OVER = 0x70,
        OP_2ROT = 0x71,
        OP_2SWAP = 0x72,
        OP_IFDUP = 0x73,
        OP_DEPTH = 0x74,
        OP_DROP = 0x75,
        OP_DUP = 0x76,
        OP_NIP = 0x77,
        OP_OVER = 0x78,
        OP_PICK = 0x79,
        OP_ROLL = 0x7a,
        OP_ROT = 0x7b,
        OP_SWAP = 0x7c,
        OP_TUCK = 0x7d,

        // splice ops
        OP_CAT = 0x7e,
        OP_SUBSTR = 0x7f,
        OP_LEFT = 0x80,
        OP_RIGHT = 0x81,
        OP_SIZE = 0x82,

        // bit logic
        OP_INVERT = 0x83,
        OP_AND = 0x84,
        OP_OR = 0x85,
        OP_XOR = 0x86,
        OP_EQUAL = 0x87,
        OP_EQUALVERIFY = 0x88,
        OP_RESERVED1 = 0x89,
        OP_RESERVED2 = 0x8a,

        // numeric
        OP_1ADD = 0x8b,
        OP_1SUB = 0x8c,
        OP_2MUL = 0x8d,
        OP_2DIV = 0x8e,
        OP_NEGATE = 0x8f,
        OP_ABS = 0x90,
        OP_NOT = 0x91,
        OP_0NOTEQUAL = 0x92,

        OP_ADD = 0x93,
        OP_SUB = 0x94,
        OP_MUL = 0x95,
        OP_DIV = 0x96,
        OP_MOD = 0x97,
        OP_LSHIFT = 0x98,
        OP_RSHIFT = 0x99,

        OP_BOOLAND = 0x9a,
        OP_BOOLOR = 0x9b,
        OP_NUMEQUAL = 0x9c,
        OP_NUMEQUALVERIFY = 0x9d,
        OP_NUMNOTEQUAL = 0x9e,
        OP_LESSTHAN = 0x9f,
        OP_GREATERTHAN = 0xa0,
        OP_LESSTHANOREQUAL = 0xa1,
        OP_GREATERTHANOREQUAL = 0xa2,
        OP_MIN = 0xa3,
        OP_MAX = 0xa4,

        OP_WITHIN = 0xa5,

        // crypto
        OP_RIPEMD160 = 0xa6,
        OP_SHA1 = 0xa7,
        OP_SHA256 = 0xa8,
        OP_HASH160 = 0xa9,
        OP_HASH256 = 0xaa,
        OP_CODESEPARATOR = 0xab,
        OP_CHECKSIG = 0xac,
        OP_CHECKSIGVERIFY = 0xad,
        OP_CHECKMULTISIG = 0xae,
        OP_CHECKMULTISIGVERIFY = 0xaf,

        // expansion
        OP_NOP1 = 0xb0,
        OP_CHECKLOCKTIMEVERIFY = 0xb1,
        OP_NOP2 = OP_CHECKLOCKTIMEVERIFY,
        OP_CHECKSEQUENCEVERIFY = 0xb2,
        OP_NOP3 = OP_CHECKSEQUENCEVERIFY,
        OP_NOP4 = 0xb3,
        OP_NOP5 = 0xb4,
        OP_NOP6 = 0xb5,
        OP_NOP7 = 0xb6,
        OP_NOP8 = 0xb7,
        OP_NOP9 = 0xb8,
        OP_NOP10 = 0xb9,

        OP_INVALIDOPCODE = 0xff,
    }
}
