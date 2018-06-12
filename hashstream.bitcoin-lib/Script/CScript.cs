using hashstream.bitcoin_lib.BlockChain;
using hashstream.bitcoin_lib.P2P;
using System;
using System.Collections.Generic;

namespace hashstream.bitcoin_lib.Script
{
    public class CScriptFrame
    {
        public OpCode Op { get; set; }
        public bool IsOp { get; set; }

        public byte[] Data { get; set; }
        public bool IsData { get; set; }

        public Int32 Number { get; set; }
        public bool IsNumber { get; set; }

        public CScriptFrame(OpCode op)
        {
            Op = op;
            IsOp = true;
        }

        public CScriptFrame(byte[] data)
        {
            Data = data;
            IsData = true;
        }

        public CScriptFrame(Int32 num)
        {
            Number = num;
            IsNumber = true;
        }

        public override string ToString()
        {
            if (IsOp)
            {
                return Op.ToString();
            }
            else if (IsNumber)
            {
                return Number.ToString();
            }
            else
            {
                return BitConverter.ToString(Data).Replace("-", "").ToLower();
            }
        }
    }

    public class CScript : IStreamable
    {
        // Maximum number of bytes pushable to the stack
        static uint MAX_SCRIPT_ELEMENT_SIZE => 520;

        // Maximum number of non-push operations per script
        static int MAX_OPS_PER_SCRIPT => 201;

        // Maximum number of public keys per multisig
        static int MAX_PUBKEYS_PER_MULTISIG => 20;

        // Maximum script length in bytes
        static int MAX_SCRIPT_SIZE => 10000;

        // Maximum number of values on script interpreter stack
        static int MAX_STACK_SIZE => 1000;

        public VarInt Length { get; set; }
        public byte[] Script { get; set; }

        public CScriptFrame[] ParsedScript { get; set; }

        public CScript() { }

        public CScript(byte[] data)
        {
            Script = data;
            Length = new VarInt((ulong)data.LongLength);

            ParsedScript = ParseScript();
        }

        public void ReadFromPayload(byte[] data, int offset)
        {
            Length = new VarInt(0);
            Length.ReadFromPayload(data, offset);

            Script = new byte[Length];
            Array.Copy(data, offset + Length.Size, Script, 0, Script.Length);
        }

        public byte[] ToArray()
        {
            var ret = new byte[Length.Size + Length];
            Array.Copy(Length.ToArray(), ret, Length.Size);
            Array.Copy(Script, 0, ret, Length.Size, Script.Length);

            return ret;
        }

        public bool ValidateRedeemScript(CScript s)
        {
            var script = new List<CScriptFrame>();
            script.AddRange(s.ParsedScript);
            script.AddRange(ParsedScript);

            return ValidateParsedScript(script);
        }

        public bool ValidateParsedScript(List<CScriptFrame> script)
        {
            var stack = new Stack<CScriptFrame>();
            var alt_stack = new Stack<CScriptFrame>();

            for (var x = 0; x < script.Count; x++)
            {
                var fx = script[x];

                if (fx.IsData || fx.IsNumber)
                {
                    stack.Push(fx);
                }
                else
                {
                    switch (fx.Op)
                    {
                        // push constants
                        case OpCode.OP_1NEGATE: stack.Push(new CScriptFrame(-1)); break;
                        case OpCode.OP_1: stack.Push(new CScriptFrame(1)); break;
                        case OpCode.OP_2: stack.Push(new CScriptFrame(2)); break;
                        case OpCode.OP_3: stack.Push(new CScriptFrame(3)); break;
                        case OpCode.OP_4: stack.Push(new CScriptFrame(4)); break;
                        case OpCode.OP_5: stack.Push(new CScriptFrame(5)); break;
                        case OpCode.OP_6: stack.Push(new CScriptFrame(6)); break;
                        case OpCode.OP_7: stack.Push(new CScriptFrame(7)); break;
                        case OpCode.OP_8: stack.Push(new CScriptFrame(8)); break;
                        case OpCode.OP_9: stack.Push(new CScriptFrame(9)); break;
                        case OpCode.OP_10: stack.Push(new CScriptFrame(10)); break;
                        case OpCode.OP_11: stack.Push(new CScriptFrame(11)); break;
                        case OpCode.OP_12: stack.Push(new CScriptFrame(12)); break;
                        case OpCode.OP_13: stack.Push(new CScriptFrame(13)); break;
                        case OpCode.OP_14: stack.Push(new CScriptFrame(14)); break;
                        case OpCode.OP_15: stack.Push(new CScriptFrame(15)); break;
                        case OpCode.OP_16: stack.Push(new CScriptFrame(16)); break;

                        //flow control
                        //TODO: this..
                        case OpCode.OP_NOP: break;
                        case OpCode.OP_ENDIF: break;
                        case OpCode.OP_IF:
                        case OpCode.OP_NOTIF:
                        case OpCode.OP_ELSE:
                            {
                                //take everything off, will implement later
                                CScriptFrame f = stack.Peek();
                                while(!f.IsOp || (f.IsOp && f.Op != OpCode.OP_ENDIF))
                                {
                                    f = stack.Pop();
                                }
                                break;
                            }

                        //stack ops
                        case OpCode.OP_TOALTSTACK: alt_stack.Push(stack.Pop()); break;
                        case OpCode.OP_FROMALTSTACK: stack.Push(alt_stack.Pop()); break;
                        case OpCode.OP_DROP: stack.Pop(); break;
                        case OpCode.OP_DUP: stack.Push(stack.Peek()); break;
                        case OpCode.OP_IFDUP:
                            {
                                var eval = stack.Pop();
                                if(eval.IsNumber && eval.Number != 0)
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
                                var zstack = new Stack<CScriptFrame>();
                                for(var y = 1; y < z.Number; y++)
                                {
                                    zstack.Push(stack.Pop());
                                }

                                var n = stack.Peek();
                                foreach(var s in zstack)
                                {
                                    stack.Push(s);
                                }
                                stack.Push(n);
                                break;
                            }
                    }
                }
            }

            return true;
        }

        public CScriptFrame[] ParseScript()
        {
            var ret = new List<CScriptFrame>();

            if (Script.Length > 0)
            {
                for (var x = 0; x < Script.Length; x++)
                {
                    var op = (OpCode)Script[x];

                    //if the first byte of the script is not an opcode, its the length of some data
                    //swap the op code for OP_0 and set x = -1 so we can read it like normal
                    //also dont put this to our stack as its not really in the script
                    if (x == 0 && !Enum.IsDefined(typeof(OpCode), (int)Script[x]))
                    {
                        op = OpCode.OP_0;
                        x = -1;
                    }
                    else
                    {
                        ret.Add(new CScriptFrame(op));
                    }

                    switch (op)
                    {
                        case OpCode.OP_0:
                        case OpCode.OP_PUSHDATA1:
                        case OpCode.OP_PUSHDATA2:
                        case OpCode.OP_PUSHDATA4:
                        case OpCode.OP_RIPEMD160:
                        case OpCode.OP_SHA1:
                        case OpCode.OP_SHA256:
                        case OpCode.OP_HASH160:
                        case OpCode.OP_HASH256:
                            {
                                var len = Script[x + 1];
                                var datastart = x + 2;
                                if (datastart + len > Script.Length)
                                {
                                    throw new Exception($"Invalid script, out of bounds for {op.ToString()}({len})");
                                }
                                var data = new byte[len];
                                Array.Copy(Script, datastart, data, 0, data.Length);

                                ret.Add(new CScriptFrame(data));
                                x += len + 1;
                                break;
                            }
                    }
                }
            }

            return ret.ToArray();
        }

        public override string ToString()
        {
            return string.Join<CScriptFrame>(" ", ParsedScript);
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
