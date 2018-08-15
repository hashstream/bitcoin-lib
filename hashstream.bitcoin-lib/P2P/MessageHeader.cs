using System;
using System.IO;

#if NETCOREAPP2_1
using System.Buffers;
#endif

namespace hashstream.bitcoin_lib.P2P
{
    public class MessageHeader : IStreamable, ICommand
    {
        public UInt32 Magic { get; set; } = 0xf9beb4d9;
        public string Command { get; set; }
        public UInt32 PayloadSize { get; set; }
        public UInt32 Checksum { get; set; }

        public int Size => StaticSize;

        public static int StaticSize => 24;

#if NETCOREAPP2_1
        public static Memory<byte> ToCommand<T>(T msg) where T : ICommand, IStreamable
        {
            if (msg != null)
            {
                var header = new MessageHeader();
                header.Command = msg.Command;
                header.PayloadSize = (uint)msg.Size;

                var packed_msg = MemoryPool<byte>.Shared.Rent(msg.Size + header.Size).Memory;
                var body_slice = packed_msg.Span.Slice(header.Size);
                msg.WriteToPayload(body_slice);

                //create the checksum of the payload
                var body_hash = body_slice.SHA256d();
                header.Checksum = BitConverter.ToUInt32(new byte[] { body_hash[0], body_hash[1], body_hash[2], body_hash[3] });

                header.WriteToPayload(packed_msg.Span);

                return packed_msg;
            }
            else
            {
                return Memory<byte>.Empty;
            }
        }

        public ReadOnlySpan<byte> ReadFromPayload(ReadOnlySpan<byte> data)
        {
            var next = data.ReadAndSlice(out UInt32 tMagic)
                .ReadAndSlice(12, out string tCommand)
                .ReadAndSlice(out UInt32 tPayloadSize)
                .ReadAndSlice(out UInt32 tChecksum);

            Magic = tMagic;
            Command = tCommand;
            PayloadSize = tPayloadSize;
            Checksum = tChecksum;

            return next;
        }

        public Span<byte> WriteToPayload(Span<byte> dest)
        {
            return dest.WriteAndSlice(Magic)
                .WriteAndSlice(Command)
                .WriteAndSlice(PayloadSize)
                .WriteAndSlice(Checksum);
        }

        public byte[] ToArray()
        {
            var ret = new byte[Size];
            WriteToPayload(ret);
            return ret;
        }
#else
        public static byte[] ToCommand<T>(T msg) where T : ICommand, IStreamable
        {
            if (msg != null)
            {
                using (var ms = new MemoryStream())
                {
                    var mp = msg.ToArray();
                    var header = new MessageHeader();
                    header.Command = msg.Command;
                    header.PayloadSize = (uint)mp.Length;

                    //create the checksum of the payload
                    var h2 = mp.SHA256d();
                    header.Checksum = BitConverter.ToUInt32(new byte[] { h2[0], h2[1], h2[2], h2[3] }, 0);

                    var hp = header.ToArray();
                    ms.Write(hp, 0, hp.Length);
                    ms.Write(mp, 0, mp.Length);

                    return ms.ToArray();
                }
            }
            else
            {
                return null;
            }
        }

        public int ReadFromPayload(byte[] data, int offset)
        {
            var roffset = offset;

            Magic = data.ReadUInt32FromBuffer(ref roffset);
            Command = data.ReadASCIIFromBuffer(ref roffset, 12);
            PayloadSize = data.ReadUInt32FromBuffer(ref roffset);
            Checksum = data.ReadUInt32FromBuffer(ref roffset);

            return Size;
        }

        public byte[] ToArray()
        {
            var woffset = 0;
            var ret = new byte[Size];
            
            ret.CopyAndIncr(BitConverter.GetBytes(Magic), ref woffset, true);
            ret.CopyAndIncr(System.Text.Encoding.ASCII.GetBytes(Command), ref woffset);
            ret.CopyAndIncr(new byte[12 - Command.Length], ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(PayloadSize), ref woffset);
            ret.CopyAndIncr(BitConverter.GetBytes(Checksum), woffset);

            return ret;
        }
#endif
    }
}
