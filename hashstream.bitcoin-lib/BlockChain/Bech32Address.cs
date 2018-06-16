using hashstream.bitcoin_lib.Encoding;
using System;
using System.Collections.Generic;

namespace hashstream.bitcoin_lib.BlockChain
{
    public enum Bech32AddressType
    {
        PubKey,
        Script
    }

    public class Bech32Address : Address
    {
        public byte[] WitnessProgram { get; internal set; }

        public int WitnessVersion { get; internal set; }

        public string Hrp { get; internal set; }

        public Bech32Address(string addr) : base(0)
        {
            var data = Bech32.Decode(addr);
            WitnessVersion = data.Item2[0];
            Hrp = data.Item1;

            //Decoded bytes contains version byte, remove this
            var pg = new byte[data.Item2.Length - 1];
            Array.Copy(data.Item2, 1, pg, 0, pg.Length);

            //convert back to witness program
            WitnessProgram = ConvertBits(pg, 5, 8, false);
            Network = GetNetworkFromHrp(Hrp);

            ValidateAddress();
        }

        public Bech32Address(AddressNetwork net, int version, byte[] program) : base(net)
        {
            WitnessVersion = version;
            WitnessProgram = program;

            Hrp = GetHrpFromNetwork(net);
            ValidateAddress();
        }

        private string GetHrpFromNetwork(AddressNetwork net)
        {
            switch (net)
            {
                case AddressNetwork.Main: return "bc";
                case AddressNetwork.Test:
                case AddressNetwork.RegTest: return "tb";
            }

            return null;
        }

        private AddressNetwork GetNetworkFromHrp(string hrp)
        {
            switch (hrp)
            {
                case "bc": return AddressNetwork.Main;
                case "tb": return AddressNetwork.Test;
            }

            return 0;
        }

        private void ValidateAddress()
        {
            if (Hrp != "bc" && Hrp != "tb")
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
        private static byte[] ConvertBits(byte[] data, int frombits, int tobits, bool pad)
        {
            var acc = 0;
            var bits = 0;
            var ret = new List<byte>();
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
                    ret.Add((byte)((acc >> bits) & maxv));
                }
            }
            if (pad)
            {
                if (bits > 0)
                {
                    ret.Add((byte)((acc << (tobits - bits)) & maxv));
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
            var data = new byte[] { (byte)WitnessVersion }.Concat(ConvertBits(WitnessProgram, 8, 5, true));
            return $"{Hrp}1{Bech32.Encode(Hrp, data)}";
        }

        public override byte[] ToArray()
        {
            return WitnessProgram;
        }
    }
}
