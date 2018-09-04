using System;
using System.Collections.Generic;
using System.Text;

#if NETCOREAPP2_1
using System.Buffers.Binary;
#endif

namespace hashstream.bitcoin_lib.BlockChain
{
    public class UInt48 : IStreamable, IEquatable<UInt64>
    {
        public UInt64 Value { get; set; }

        public int Size => 6;

        public UInt48() { }

        public UInt48(UInt64 v)
        {
            Value = v;
        }

#if NETCOREAPP2_1
        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(Size, out byte[] tValue);

            var stmp = new Span<byte>(new byte[8]);
            tValue.AsSpan().CopyTo(stmp);

            Value = BinaryPrimitives.ReadUInt64LittleEndian(stmp);

            return next;
        }

        public byte[] ToArray()
        {
            var ret = new Span<byte>(new byte[8]);
            BinaryPrimitives.WriteUInt64LittleEndian(ret, Value);
            return ret.Slice(0, Size).ToArray();
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(ToArray());
        }
#else
        public int ReadFromPayload(byte[] data, int offset = 0)
        {
            var tValue = new byte[8];
            Array.Copy(data, offset, tValue, 0, Size);
            
            Value = BitConverter.ToUInt64(tValue, 0);

            return 6;
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];
            var tValue = BitConverter.GetBytes(Value);
            Array.Copy(tValue, 0, ret, 0, Size);
            return ret;
        }
#endif

        public static implicit operator int(UInt48 v)
        {
            return (int)v.Value;
        }

        public static implicit operator UInt48(int x)
        {
            return new UInt48((UInt64)x);
        }

        public static int operator +(UInt48 a, int b)
        {
            return (int)a.Value + b;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
        
        public bool Equals(ulong other)
        {
            return Value == other;
        }
    }
}
