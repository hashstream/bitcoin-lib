using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace hashstream.bitcoin_lib.P2P
{
    public class MessageHeader : IStreamable
    {
        public uint Magic { get; set; } = 0xf9beb4d9;
        public string Command { get; set; }
        public uint PayloadSize { get; set; }
        public byte[] Checksum { get; set; }

        public static byte[] ToCommand(IStreamable msg)
        {
            using (var ms = new MemoryStream())
            {
                var mp = msg.ToArray();
                var header = new MessageHeader();
                header.Command = msg.Command;
                header.PayloadSize = (uint)mp.Length;

                //create the checksum of the payload
                using (var sha = SHA256.Create())
                {
                    var h1 = sha.ComputeHash(mp);
                    var h2 = sha.ComputeHash(h1);

                    header.Checksum = new byte[] { h2[0], h2[1], h2[2], h2[3] };
                }

                var hp = header.ToArray();
                ms.Write(hp, 0, hp.Length);
                ms.Write(mp, 0, mp.Length);

                return ms.ToArray();
            }
        }

        public void ReadFromPayload(byte[] header, int offset)
        {
            Magic = BitConverter.ToUInt32(header, offset);
            Command = Encoding.ASCII.GetString(header, offset + 4, 12);
            PayloadSize = BitConverter.ToUInt32(header, offset + 16);
            Checksum = new byte[] { header[20], header[21], header[22], header[23] };
        }

        public byte[] ToArray()
        {
            var pl = new byte[][]
            {
                BitConverter.GetBytes(Magic),
                Encoding.ASCII.GetBytes(Command),
                new byte[12-Command.Length],
                BitConverter.GetBytes(PayloadSize),
                Checksum
            };

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(pl[0]);
            }

            using (var ms = new MemoryStream())
            {
                foreach (var b in pl)
                {
                    ms.Write(b, 0, b.Length);
                }

                return ms.ToArray();
            }
        }
    }
}
