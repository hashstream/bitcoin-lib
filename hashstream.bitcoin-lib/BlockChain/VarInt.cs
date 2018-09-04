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

        public int Size
        {
            get
            {
                if (Value < 253)
                {
                    return 1;
                }
                else if (Value <= UInt16.MaxValue)
                {
                    return 3;
                }
                else if (Value <= UInt32.MaxValue)
                {
                    return 5;
                }
                else
                {
                    return 9;
                }
            }
        }
        
        public VarInt()
        {

        }

        public VarInt(UInt64 v)
        {
            Value = v;
        }

        public bool Equals(ulong other)
        {
            return Value == other;
        }

#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var b1 = data[0];
            var ret = data.Slice(1);
            
            if (b1 < 253)
            {
                Value = b1;
            }
            else if (b1 == 0xfd)
            {
                ret = ret.ReadAndSlice(out UInt16 tVal);
                Value = tVal;
            }
            else if (b1 == 0xfe)
            {
                ret = ret.ReadAndSlice(out UInt32 tVal);
                Value = tVal;
            }
            else if (b1 == 0xff)
            {
                ret = ret.ReadAndSlice(out UInt64 tVal);
                Value = tVal;
            }
            else
            {
                throw new Exception("WTF!");
            }

            return ret;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            if (Value < 0xfd)
            {
                return dest.WriteAndSlice((byte)Value);
            }
            else if (Value <= UInt16.MaxValue)
            {
                return dest.WriteAndSlice((byte)0xfd).WriteAndSlice((UInt16)Value);
            }
            else if (Value <= UInt32.MaxValue)
            {
                return dest.WriteAndSlice((byte)0xfe).WriteAndSlice((UInt16)Value);
            }
            else
            {
                return dest.WriteAndSlice((byte)0xff).WriteAndSlice((UInt16)Value);
            }
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];
            WriteToPayload(ret);
            return ret;
        }
#else
        public int ReadFromPayload(byte[] header, int offset = 0)
        {
            var b1 = header[offset];

            if (b1 < 253)
            {
                Value = (UInt64)b1;
            }
            else if (b1 == 0xfd)
            {
                Value = BitConverter.ToUInt16(header, offset + 1);
            }
            else if (b1 == 0xfe)
            {
                Value = BitConverter.ToUInt32(header, offset + 1);
            }
            else if (b1 == 0xff)
            {
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
#endif

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
