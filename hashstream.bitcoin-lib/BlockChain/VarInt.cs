using hashstream.bitcoin_lib.P2P;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace hashstream.bitcoin_lib.BlockChain
{
    public class VarInt : IStreamable, IEquatable<UInt64>
    {
        public UInt64 Value { get; set; }
        public int Size { get; private set; }

        public VarInt() { }

        public VarInt(UInt64 v)
        {
            Value = v;
            if (v < 253)
            {
                Size = 1;
            }
            else if (v <= UInt16.MaxValue)
            {
                Size = 3;
            }
            else if (v <= UInt32.MaxValue)
            {
                Size = 5;
            }
            else
            {
                Size = 9;
            }
        }

        public bool Equals(ulong other)
        {
            return Value == other;
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.UInt64;
        }

        public int ReadFromPayload(byte[] header, int offset)
        {
            var b1 = header[offset];

            if (b1 < 253)
            {
                Size = 1;
                Value = (UInt64)b1;
            }
            else if (b1 == 0xfd)
            {
                Size = 3;
                Value = BitConverter.ToUInt16(header, offset + 1);
            }
            else if (b1 == 0xfe)
            {
                Size = 5;
                Value = BitConverter.ToUInt32(header, offset + 1);
            }
            else if (b1 == 0xff)
            {
                Size = 9;
                Value = BitConverter.ToUInt64(header, offset + 1);
            }
            else
            {
                throw new Exception("WTF!");
            }

            return Size;
        }

        public byte[] ToArray()
        {
            using (var ms = new MemoryStream())
            {
                if (Value < 0xfd)
                {
                    ms.WriteByte((byte)Value);
                }
                else if (Value <= UInt16.MaxValue)
                {
                    ms.WriteByte(0xfd);
                    var p = BitConverter.GetBytes((UInt16)Value);
                    ms.Write(p, 0, p.Length);
                }
                else if (Value <= UInt32.MaxValue)
                {
                    ms.WriteByte(0xfe);
                    var p = BitConverter.GetBytes((UInt32)Value);
                    ms.Write(p, 0, p.Length);
                }
                else
                {
                    ms.WriteByte(0xff);
                    var p = BitConverter.GetBytes((UInt64)Value);
                    ms.Write(p, 0, p.Length);
                }

                return ms.ToArray();
            }
        }

        public static implicit operator int(VarInt v)
        {
            return (int)v.Value;
        }

        public static implicit operator VarInt(int x)
        {
            return new VarInt((UInt64)x);
        }

        public static int operator +(VarInt a, int b)
        {
            return (int)a.Value + b;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
