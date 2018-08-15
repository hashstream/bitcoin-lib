using System;
using System.Runtime.CompilerServices;

namespace hashstream.bitcoin_lib
{
    public interface IStreamable
    {
        /// <summary>
        /// Reads the object off a byte[] buffer and returns its length
        /// </summary>
        /// <param name="data">The buffer to read from</param>
        /// <param name="offset">The offset in the buffer to start reading</param>
        /// <returns>Length of data read from buffer</returns>

#if NETCOREAPP2_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data);
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int ReadFromPayload(byte[] data, int offset);
#endif

        /// <summary>
        /// Serializes the object into the Span<byte>
        /// </summary>
        /// <param name="dest">destination span</param>
        /// <returns>a new span starting from the end of the serialized message</returns>
#if NETCOREAPP2_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Span<byte> WriteToPayload(Span<byte> dest);
#endif

        /// <summary>
        /// Serializes the object into a byte[]
        /// </summary>
        /// <returns>The serialized buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        byte[] ToArray();


        int Size { get; }
    }
}
