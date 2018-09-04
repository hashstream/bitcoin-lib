using hashstream.bitcoin_lib.P2P;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using hashstream.bitcoin_lib.BlockChain;

#if NETCOREAPP2_1
using System.Buffers.Binary;
using RIPMD160Mgd = hashstream.bitcoin_lib.Crypto.RIPEMD160Managed;
#else 
using RIPMD160Mgd = System.Security.Cryptography.RIPEMD160Managed;
#endif

namespace hashstream.bitcoin_lib
{
    public static class Extensions
    {
        private static Dictionary<string, byte> HexTable { get; set; } = BuildHexTable();

        private static Dictionary<string, byte> BuildHexTable()
        {
            var ret = new Dictionary<string, byte>();
            for(var x = 0; x <= 255; x++)
            {
                ret.Add(x.ToString("X2"), (byte)x);
            }

            return ret;
        }

        public static byte[] FromHex(this string s)
        {
            if(s.Length % 2 != 0)
            {
                throw new Exception("Invalid hex string");
            }

            s = s.ToUpper();

            var ret = new byte[s.Length / 2];
            for(var x = 0; x < s.Length; x += 2)
            {
                ret[x / 2] = HexTable[$"{s[x]}{s[x + 1]}"];
            }

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToHex(this byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", "").ToLower();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Concat<T>(this T[] a, T[] b)
        {
            var ret = new T[a.Length + b.Length];
            Array.Copy(a, ret, a.Length);
            Array.Copy(b, 0, ret, a.Length, b.Length);
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] SHA256d(this byte[] data)
        {
            byte[] hash = null;
            using (var sha = new SHA256Managed())
            {
                hash = sha.ComputeHash(data);
                hash = sha.ComputeHash(hash);
            }

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] SHA256(this byte[] data)
        {
            using (var sha = new SHA256Managed())
            {
                return sha.ComputeHash(data);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] RIPEMD160(this byte[] data)
        {
            using (var rmd = new RIPMD160Mgd())
            {
                return rmd.ComputeHash(data);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Hash160(this byte[] data)
        {
            return data.SHA256().RIPEMD160();
        }

#if NETCOREAPP2_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> SHA256d<T>(T obj) where T : IStreamable
        {
            var tpl = new Span<byte>(new byte[obj.Size]);
            obj.WriteToPayload(tpl);
            return tpl.SHA256d();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToHex(this Span<byte> data)
        {
            return BitConverter.ToString(data.ToArray()).Replace("-", "").ToLower();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> SHA256d(this ReadOnlySpan<byte> data)
        {
            using (var sha = new SHA256Managed())
            {
                //internally this is copied to the destination so its ok to reuse the destination twice
                //https://github.com/dotnet/corefx/blob/57608f6a5cfeadf338dc6c5d0300147b39168012/src/System.Security.Cryptography.Primitives/src/System/Security/Cryptography/HashAlgorithm.cs#L244
                var h1 = new Span<byte>(new byte[sha.HashSize / 8]);
                sha.TryComputeHash(data, h1, out int _);
                sha.TryComputeHash(h1, h1, out int _);

                return h1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> SHA256d(this Span<byte> data)
        {
            return ((ReadOnlySpan<byte>)data).SHA256d();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<int> ReadAsyncExact(this Stream s, Memory<byte> buf)
        {
            return await s.ReadAsync(buf);
        }

        // Write ops
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> WriteAndSlice<T>(this Span<byte> dest, T[] obj) where T : IStreamable
        {
            var ret = dest;
            foreach(var x in obj)
            {
                ret = ret.WriteAndSlice<T>(x);
            }

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> WriteAndSlice<T>(this Span<byte> dest, T obj) where T : IStreamable
        {
            return obj.WriteToPayload(dest);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> WriteAndSlice(this Span<byte> dest, byte[] obj)
        {
            obj.AsSpan().CopyTo(dest);
            return dest.Slice(obj.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> WriteAndSlice(this Span<byte> dest, byte obj)
        {
            dest[0] = obj;
            return dest.Slice(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> WriteAndSlice(this Span<byte> dest, UInt16 obj)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(dest, obj);
            return dest.Slice(2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> WriteAndSlice(this Span<byte> dest, UInt32 obj)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(dest, obj);
            return dest.Slice(4);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> WriteAndSlice(this Span<byte> dest, UInt64 obj)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(dest, obj);
            return dest.Slice(8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> WriteAndSlice(this Span<byte> dest, Int16 obj)
        {
            BinaryPrimitives.WriteInt16LittleEndian(dest, obj);
            return dest.Slice(2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> WriteAndSlice(this Span<byte> dest, Int32 obj)
        {
            BinaryPrimitives.WriteInt32LittleEndian(dest, obj);
            return dest.Slice(4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> WriteAndSlice(this Span<byte> dest, Int64 obj)
        {
            BinaryPrimitives.WriteInt64LittleEndian(dest, obj);
            return dest.Slice(8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> WriteAndSlice(this Span<byte> dest, IPAddress obj)
        {
            obj.MapToIPv6().GetAddressBytes().AsSpan().CopyTo(dest);
            return dest.Slice(16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> WriteAndSlice(this Span<byte> dest, string obj, int len = -1)
        {
            var bd = System.Text.Encoding.ASCII.GetBytes(obj);
            bd.AsSpan().CopyTo(dest);
            return dest.Slice(len == -1 ? bd.Length : len);
        }

        // Read ops
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> ReadAndSlice<T>(this ReadOnlySpan<byte> src, int nItems, out T[] obj) where T : IStreamable, new()
        {
            var ret = src;
            obj = new T[nItems];
            for(var x =0;x<nItems;x++)
            {
                var xn = new T();
                ret = xn.ReadFromPayload(ret);
                obj[x] = xn;
            }

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> ReadAndSlice<T>(this ReadOnlySpan<byte> src, out T obj) where T : IStreamable, new()
        {
            obj = new T();
            return obj.ReadFromPayload(src);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> ReadAndSlice(this ReadOnlySpan<byte> buf, out byte obj)
        {
            obj = buf[0];
            return buf.Slice(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> ReadAndSlice(this ReadOnlySpan<byte> buf, int len, out byte[] obj)
        {
            obj = buf.Slice(0, len).ToArray();
            return buf.Slice(len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> ReadAndSlice(this ReadOnlySpan<byte> buf, out UInt16 obj)
        {
            obj = BinaryPrimitives.ReadUInt16LittleEndian(buf);
            return buf.Slice(2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> ReadAndSlice(this ReadOnlySpan<byte> buf, out UInt32 obj)
        {
            obj = BinaryPrimitives.ReadUInt32LittleEndian(buf);
            return buf.Slice(4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> ReadAndSlice(this ReadOnlySpan<byte> buf, out UInt64 obj)
        {
            obj = BinaryPrimitives.ReadUInt64LittleEndian(buf);
            return buf.Slice(8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> ReadAndSlice(this ReadOnlySpan<byte> buf, out Int16 obj)
        {
            obj = BinaryPrimitives.ReadInt16LittleEndian(buf);
            return buf.Slice(2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> ReadAndSlice(this ReadOnlySpan<byte> buf, out Int32 obj)
        {
            obj = BinaryPrimitives.ReadInt32LittleEndian(buf);
            return buf.Slice(4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> ReadAndSlice(this ReadOnlySpan<byte> buf, out Int64 obj)
        {
            obj = BinaryPrimitives.ReadInt64LittleEndian(buf);
            return buf.Slice(8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> ReadAndSlice(this ReadOnlySpan<byte> buf, out IPAddress obj)
        {
            obj = new IPAddress(buf.Slice(0, 16));
            return buf.Slice(16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> ReadAndSlice(this ReadOnlySpan<byte> buf, int len, out string obj)
        {
            obj = System.Text.Encoding.ASCII.GetString(buf.Slice(0, len));
            return buf.Slice(len);
        }

#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<int> ReadAsyncExact(this Stream s, byte[] buf, int offset, int count)
        {
            var i_offset = 0;

            read_more:
            var rlen = await s.ReadAsync(buf, offset + i_offset, count - i_offset);
            if (rlen == 0)
                return 0;

            if(i_offset + rlen < count - i_offset)
            {
                i_offset += rlen;
                goto read_more;
            }

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyAndIncr<T>(this byte[] dest, T src, ref int offset) where T : IStreamable
        {
            dest.CopyAndIncr(src.ToArray(), ref offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyAndIncr<T>(this byte[] dest, T[] src, ref int offset) where T : IStreamable
        {
            foreach (var x in src)
            {
                dest.CopyAndIncr(x.ToArray(), ref offset);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyAndIncr(this byte[] dest, byte[] src, ref int offset, bool reverse = false)
        {
            if (reverse)
            {
                Array.Reverse(src);
            }
            Array.Copy(src, 0, dest, offset, src.Length);
            offset += src.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyAndIncr(this byte[] dest, byte[] src, int offset, bool reverse = false)
        {
            if (reverse)
            {
                Array.Reverse(src);
            }
            Array.Copy(src, 0, dest, offset, src.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadFromBuffer<T>(this byte[] src, ref int offset) where T : IStreamable, new()
        {
            var ret = new T();
            offset += ret.ReadFromPayload(src, offset);

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadFromBuffer<T>(this byte[] src) where T : IStreamable, new()
        {
            var ret = new T();
            ret.ReadFromPayload(src, 0);

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] ReadFromBuffer<T>(this byte[] src, int count, ref int offset) where T : IStreamable, new()
        {
            var ret = new T[count];
            for (var x = 0; x < count; x++)
            {
                ret[x] = new T();
                offset += ret[x].ReadFromPayload(src, offset);
            }

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] ReadFromBuffer<T>(this byte[] src, int count) where T : IStreamable, new()
        {
            var ret = new T[count];
            var ioffset = 0;
            for (var x = 0; x < count; x++)
            {
                ret[x] = new T();
                ioffset += ret[x].ReadFromPayload(src, ioffset);
            }

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt16 ReadUInt16FromBuffer(this byte[] src, ref int offset)
        {
            var ret = BitConverter.ToUInt16(src, offset);
            offset += 2;

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt32 ReadUInt32FromBuffer(this byte[] src, ref int offset)
        {
            var ret = BitConverter.ToUInt32(src, offset);
            offset += 4;

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UInt64 ReadUInt64FromBuffer(this byte[] src, ref int offset)
        {
            var ret = BitConverter.ToUInt64(src, offset);
            offset += 8;

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int16 ReadInt16FromBuffer(this byte[] src, ref int offset)
        {
            var ret = BitConverter.ToInt16(src, offset);
            offset += 2;

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 ReadInt32FromBuffer(this byte[] src, ref int offset)
        {
            var ret = BitConverter.ToInt32(src, offset);
            offset += 4;

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int64 ReadInt64FromBuffer(this byte[] src, ref int offset)
        {
            var ret = BitConverter.ToInt64(src, offset);
            offset += 8;

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IPAddress ReadIPAddressFromBuffer(this byte[] src, ref int offset)
        {
            var ip = new byte[16];
            Array.Copy(src, offset, ip, 0, 16);
            offset += 16;

            return new IPAddress(ip);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadASCIIFromBuffer(this byte[] src, ref int offset, int len)
        {
            var ret = System.Text.Encoding.ASCII.GetString(src, offset, len);
            offset += len;

            return ret;
        }
#endif
    }
}
