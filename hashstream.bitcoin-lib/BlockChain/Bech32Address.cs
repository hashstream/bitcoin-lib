using hashstream.bitcoin_lib.Encoding;
using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.BlockChain
{
    public class Bech32Address
    {
        private Bech32 Bech32Data { get; set; }

        public int[] WitnessProgram { get; internal set; }

        public int WitnessVersion { get; internal set; }

        public string Hrp => Bech32Data.Hrp;

        public Bech32Address(string addr)
        {
            Bech32Data = new Bech32(addr);
            WitnessVersion = Bech32Data.DataBytes[0];

            //Decoded bytes contains version byte, remove this
            var pg = new int[Bech32Data.DataBytes.Length - 1];
            Array.Copy(Bech32Data.DataBytes, 1, pg, 0, pg.Length);

            //convert back to witness program
            WitnessProgram = ConvertBits(pg, 5, 8, false);

            ValidateArress();
        }

        public Bech32Address(string hrp, int version, int[] program)
        {
            WitnessVersion = version;
            WitnessProgram = program;

            var data = new int[] { WitnessVersion }.Concat(ConvertBits(WitnessProgram, 8, 5, true));
            Bech32Data = new Bech32(hrp, data);

            ValidateArress();
        }

        private void ValidateArress()
        {
            if (Bech32Data.Hrp != "bc" && Bech32Data.Hrp != "tb")
            {
                throw new Exception("Invalid bech32 address, must start with 'bc' for mainnet or 'tb' for testnet");
            }

            if (WitnessVersion > 16)
            {
                throw new Exception("Invalid bech32 address, witness version must be between 0-16");
            }

            if (WitnessVersion == 0 && WitnessProgram.Length != 20 && WitnessProgram.Length != 32)
            {
                throw new Exception($"Invalid bech32 address, witness program v{WitnessVersion} must be either 20 or 32 bytes long");
            }
        }

        // https://github.com/sipa/bech32/blob/master/ref/javascript/segwit_addr.js#L28
        // BIP didnt really explain this so well so just copied from ref script
        private static int[] ConvertBits(int[] data, int frombits, int tobits, bool pad)
        {
            var acc = 0;
            var bits = 0;
            var ret = new List<int>();
            var maxv = (1 << tobits) - 1;
            for (var p = 0; p < data.Length; ++p)
            {
                var value = data[p];
                if (value < 0 || (value >> frombits) != 0)
                {
                    return null;
                }
                acc = (acc << frombits) | value;
                bits += frombits;
                while (bits >= tobits)
                {
                    bits -= tobits;
                    ret.Add((acc >> bits) & maxv);
                }
            }
            if (pad)
            {
                if (bits > 0)
                {
                    ret.Add((acc << (tobits - bits)) & maxv);
                }
            }
            else if (bits >= frombits || ((acc << (tobits - bits)) & maxv) != 0)
            {
                return null;
            }
            return ret.ToArray();
        }

        public override string ToString()
        {
            return Bech32Data.ToString();
        }
    }
}
