﻿using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class MessageHeader : IStreamable, ICommand
    {
        public UInt32 Magic { get; set; } = 0xf9beb4d9;
        public string Command { get; set; }
        public UInt32 PayloadSize { get; set; }
        public byte[] Checksum { get; set; }

        public static int Size => 24;

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
                    header.Checksum = new byte[] { h2[0], h2[1], h2[2], h2[3] };

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
            Checksum = new byte[] { data[roffset], data[roffset + 1], data[roffset + 2], data[roffset + 3] };

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
            ret.CopyAndIncr(Checksum, woffset);

            return ret;
        }
    }
}
