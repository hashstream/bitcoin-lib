﻿using System;
using System.Collections.Generic;

namespace hashstream.bitcoin_lib.Script
{
    public enum CScriptFrameType
    {
        OpCode,
        Data,
        Number
    }

    public class CScriptFrame
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

        public CScriptFrame(OpCode op)
        {
            Op = op;
            Type = CScriptFrameType.OpCode;
        }

        public CScriptFrame(byte[] data)
        {
            Data = data;
            Type = CScriptFrameType.Data;
        }

        public CScriptFrame(Int32 num)
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

        public static CScriptFrame operator +(CScriptFrame a, CScriptFrame b)
        {
            if (a.IsNumber && b.IsNumber)
            {
                return new CScriptFrame(a.Number + b.Number);
            }
            throw new Exception($"Cannot add non number frames Type-A={a.Type.ToString()} Type-B={b.Type.ToString()}");
        }

        public static CScriptFrame operator -(CScriptFrame a, CScriptFrame b)
        {
            if (a.IsNumber && b.IsNumber)
            {
                return new CScriptFrame(a.Number - b.Number);
            }
            throw new Exception($"Cannot add non number frames Type-A={a.Type.ToString()} Type-B={b.Type.ToString()}");
        }

        //Implicit operators
        public static implicit operator CScriptFrame(int s)
        {
            return new CScriptFrame(s);
        }

        public static implicit operator int(CScriptFrame s)
        {
            if (s.IsNumber)
            {
                return s.Number;
            }
            throw new Exception($"Cannot convert non int frame to int Type={s.Type.ToString()}");
        }

        public static implicit operator CScriptFrame(bool s)
        {
            return new CScriptFrame(s ? 1 : 0);
        }

        public static implicit operator bool(CScriptFrame s)
        {
            if (s.IsNumber)
            {
                return s.Number != 0;
            }
            throw new Exception($"Cannot convert non int frame to bool Type={s.Type.ToString()}");
        }

        public static implicit operator CScriptFrame(OpCode o)
        {
            return new CScriptFrame(o);
        }

        public static implicit operator OpCode(CScriptFrame o)
        {
            if (o.IsOp)
            {
                return o.Op;
            }
            throw new Exception($"Cannot convert non opcode frame to opcode Type={o.Type.ToString()}");
        }

        public static implicit operator CScriptFrame(byte[] o)
        {
            return new CScriptFrame(o);
        }

        public static implicit operator byte[] (CScriptFrame o)
        {
            if (o.IsData)
            {
                return o.Data;
            }
            throw new Exception($"Cannot convert non data frame to data Type={o.Type.ToString()}");
        }

        public static implicit operator CScriptFrame(string s)
        {
            return new CScriptFrame(System.Text.Encoding.UTF8.GetBytes(s));
        }

        public static implicit operator string(CScriptFrame s)
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
            if (obj is CScriptFrame)
            {
                var b = (CScriptFrame)obj;
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