using hashstream.bitcoin_lib.Encoding;
using System;
using System.Collections.Generic;
using System.Text;

namespace hashstream.bitcoin_lib.BlockChain
{
    public class Bech32Address : Bech32
    {
        public int WitnessVersion { get; internal set; }

        public Bech32Address(string addr) : base(addr)
        {
            WitnessVersion = DataBytes[0];

            //Decoded bytes contains version byte, remove this
            var pg = new int[DataBytes.Length - 1];
            Array.Copy(DataBytes, 1, pg, 0, pg.Length);

            //convert back to witness program
            DataBytes = ConvertBits(pg, 5, 8, false);

            ValidateArress();
        }

        public Bech32Address(string hrp, int version, int[] program) : base(hrp, new int[version].Concat(ConvertBits(program)))
        {
            ValidateArress();
        }

        private void ValidateArress()
        {
            if (Hrp != "bc" || Hrp != "tb")
            {
                throw new Exception("Invalid bech32 address, must start with 'bc' for mainnet or 'tb' for testnet");
            }

            if (WitnessVersion > 16)
            {
                throw new Exception("Invalid bech32 address, witness version must be between 0-16");
            }

            if (WitnessVersion == 0 && DataBytes.Length != 20 && DataBytes.Length != 32)
            {
                throw new Exception($"Invalid bech32 address, witness program v{WitnessVersion} must be either 20 or 32 bytes long");
            }
        }

        // https://github.com/sipa/bech32/blob/master/ref/javascript/segwit_addr.js#L28
        // BIP didnt really explain this so well so just copied from ref script
        private static int[] ConvertBits(int[] data, int frombits = 8, int tobits = 5, bool pad = true)
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
    }
}
