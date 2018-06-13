using System;
using System.Collections.Generic;

namespace hashstream.bitcoin_lib.Script
{
    public enum CScriptFrameType
    {
        OpCode,
        Data,
        Number
    }

    public class ScriptFrame
    {
        public CScriptFrameType Type { get; private set; }
        public OpCode Op { get; private set; }
        public bool IsOp => Type == CScriptFrameType.OpCode;

        public byte[] Data { get; private set; }
        public bool IsData => Type == CScriptFrameType.Data;

        public Int32 Number { get; private set; }
        public bool IsNumber => Type == CScriptFrameType.Number;

        public int Length
        {
            get
            {
                if (IsData)
                {
                    return Data.Length;
                }
                throw new Exception($"Cannot return length of non data from Type={Type.ToString()}");
            }
        }

        public ScriptFrame(OpCode op)
        {
            Op = op;
            Type = CScriptFrameType.OpCode;
        }

        public ScriptFrame(byte[] data)
        {
            Data = data;
            Type = CScriptFrameType.Data;
        }

        public ScriptFrame(Int32 num)
        {
            Number = num;
            Type = CScriptFrameType.Number;
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

        public static ScriptFrame operator +(ScriptFrame a, ScriptFrame b)
        {
            if (a.IsNumber && b.IsNumber)
            {
                return new ScriptFrame(a.Number + b.Number);
            }
            throw new Exception($"Cannot add non number frames Type-A={a.Type.ToString()} Type-B={b.Type.ToString()}");
        }

        public static ScriptFrame operator -(ScriptFrame a, ScriptFrame b)
        {
            if (a.IsNumber && b.IsNumber)
            {
                return new ScriptFrame(a.Number - b.Number);
            }
            throw new Exception($"Cannot add non number frames Type-A={a.Type.ToString()} Type-B={b.Type.ToString()}");
        }

        //Implicit operators
        public static implicit operator ScriptFrame(int s)
        {
            return new ScriptFrame(s);
        }

        public static implicit operator int(ScriptFrame s)
        {
            if (s.IsNumber)
            {
                return s.Number;
            }
            throw new Exception($"Cannot convert non int frame to int Type={s.Type.ToString()}");
        }

        public static implicit operator ScriptFrame(bool s)
        {
            return new ScriptFrame(s ? 1 : 0);
        }

        public static implicit operator bool(ScriptFrame s)
        {
            if (s.IsNumber)
            {
                return s.Number != 0;
            }
            throw new Exception($"Cannot convert non int frame to bool Type={s.Type.ToString()}");
        }

        public static implicit operator ScriptFrame(OpCode o)
        {
            return new ScriptFrame(o);
        }

        public static implicit operator OpCode(ScriptFrame o)
        {
            if (o.IsOp)
            {
                return o.Op;
            }
            throw new Exception($"Cannot convert non opcode frame to opcode Type={o.Type.ToString()}");
        }

        public static implicit operator ScriptFrame(byte[] o)
        {
            return new ScriptFrame(o);
        }

        public static implicit operator byte[] (ScriptFrame o)
        {
            if (o.IsData)
            {
                return o.Data;
            }
            throw new Exception($"Cannot convert non data frame to data Type={o.Type.ToString()}");
        }

        public static implicit operator ScriptFrame(string s)
        {
            return new ScriptFrame(System.Text.Encoding.UTF8.GetBytes(s));
        }

        public static implicit operator string(ScriptFrame s)
        {
            if (s.IsData)
            {
                return System.Text.Encoding.UTF8.GetString(s.Data);
            }
            throw new Exception($"Cannot convert non data frame to string Type={s.Type.ToString()}");
        }

        //comparators
        public override bool Equals(object obj)
        {
            if (obj is ScriptFrame)
            {
                var b = (ScriptFrame)obj;
                if (IsOp && b.IsOp)
                {
                    return Op == b.Op;
                }
                else if (IsNumber && b.IsNumber)
                {
                    return Number == b.Number;
                }
                else if (IsData && b.IsData && Data.Length == b.Data.Length)
                {
                    for (var x = 0; x < Data.Length; x++)
                    {
                        if (Data[x] != b.Data[x])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = -2090149594;
            hashCode = hashCode * -1521134295 + Op.GetHashCode();
            hashCode = hashCode * -1521134295 + IsOp.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(Data);
            hashCode = hashCode * -1521134295 + IsData.GetHashCode();
            hashCode = hashCode * -1521134295 + Number.GetHashCode();
            hashCode = hashCode * -1521134295 + IsNumber.GetHashCode();
            return hashCode;
        }
    }
}
