﻿#if NETCOREAPP2_1

using hashstream.bitcoin_lib;
using System;
using System.Buffers;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace hashstream.bitcoin_node_lib
{
    public class MessageStream : IDisposable
    {
        private int InternalBufferSize => 8192;

        private Stream Stream { get; set; }

        private IMemoryOwner<byte> MemoryOwner { get; set; }
        private Memory<byte> InternalBuffer { get; set; }

        private int BufferedData { get; set; }

        public MessageStream(Stream s)
        {
            MemoryOwner = MemoryPool<byte>.Shared.Rent(InternalBufferSize);
            
            InternalBuffer = MemoryOwner.Memory;
            Stream = s;
        }

        public async Task<T> ReadMessage<T>(int size, CancellationToken ct = default, UInt32? checksum = null) where T : IStreamable, new()
        {
            top:
            if (BufferedData >= size)
            {
                //we have enough data buffered
                if(checksum.HasValue)
                {
                    var chk = BitConverter.ToUInt32(InternalBuffer.Span.Slice(0, size).SHA256d());
                    if(chk != checksum.Value)
                    {
                        //skip this payload because checksum failed
                        if(BufferedData > size)
                        {
                            InternalBuffer.Span.Slice(size, BufferedData - size).CopyTo(InternalBuffer.Span);
                            BufferedData -= size;
                        }

                        throw new Exception("Checksum failed");
                    }
                }

                var obj = new T();
                obj.ReadFromPayload(InternalBuffer.Span);
                if(BufferedData > size)
                {
                    //copy extra data to start of the buffer
                    InternalBuffer.Span.Slice(size, BufferedData - size).CopyTo(InternalBuffer.Span);
                }

                BufferedData -= size;

                return obj;
            }
            else
            {
                //resize the buffer
                if (size > InternalBuffer.Length)
                {
                    if(size > MemoryPool<byte>.Shared.MaxBufferSize)
                    {
                        throw new Exception($"Buffer size is larger than the max size {MemoryPool<byte>.Shared.MaxBufferSize}");
                    }

                    var newowner = MemoryPool<byte>.Shared.Rent((int)(Math.Ceiling(size / (decimal)InternalBufferSize) * InternalBufferSize));
                    var newbuf = newowner.Memory;
                    if(BufferedData > 0)
                    {
                        InternalBuffer.Slice(0, BufferedData).CopyTo(newbuf);
                    }

                    MemoryOwner.Dispose();

                    MemoryOwner = newowner;
                    InternalBuffer = newbuf;
                }

                read_more:
                try
                {
                    var rlen = await Stream.ReadAsync(InternalBuffer.Slice(BufferedData), ct);
                    BufferedData += rlen;
                    if (rlen == 0)
                    {
                        return default;
                    }
                    else if (BufferedData < size)
                    {
                        goto read_more;
                    }
                    else
                    {
                        goto top;
                    }
                }
                catch
                {
                    return default;
                }
            }
        }

        public void Dispose()
        {
            ((IDisposable)Stream).Dispose();
            InternalBuffer = null;
            MemoryOwner.Dispose();
        }
    }
}

#endif