using System;
using System.Collections.Generic;

namespace hashstream.bitcoin_lib.Encoding
{
    /// <summary>
    /// As per BIP0173 
    /// <para>https://github.com/bitcoin/bips/blob/master/bip-0173.mediawiki</para>
    /// </summary>
    public class Bech32
    {
        private static readonly int[] Generator = new int[] { 0x3b6a57b2, 0x26508e6d, 0x1ea119fa, 0x3d4233dd, 0x2a1462b3 };
        private static readonly char[] ExcludedDataChars = new char[] { '1', 'b', 'i', 'o' };
        private static readonly string EncodingTable = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";
        private static readonly Dictionary<char, int> DecodingTable = new Dictionary<char, int>()
        {
            { 'q', 0 },
            { 'p', 1 },
            { 'z', 2 },
            { 'r', 3 },
            { 'y', 4 },
            { '9', 5 },
            { 'x', 6 },
            { '8', 7 },
            { 'g', 8 },
            { 'f', 9 },
            { '2', 10 },
            { 't', 11 },
            { 'v', 12 },
            { 'd', 13 },
            { 'w', 14 },
            { '0', 15 },
            { 's', 16 },
            { '3', 17 },
            { 'j', 18 },
            { 'n', 19 },
            { '5', 20 },
            { '4', 21 },
            { 'k', 22 },
            { 'h', 23 },
            { 'c', 24 },
            { 'e', 25 },
            { '6', 26 },
            { 'm', 27 },
            { 'u', 28 },
            { 'a', 29 },
            { '7', 30 },
            { 'l', 31 },
        };

        public string Hrp { get; internal set; }

        public string DataString { get; internal set; }

        public int[] DataBytes { get; internal set; }
        
        /// <summary>
        /// Decodes a bech32 string
        /// </summary>
        /// <param name="addr"></param>
        public Bech32(string addr)
        {
            if (addr.Length == 0 || addr.Length > 90)
            {
                throw new Exception("Invalid bech32 string, length must be between 1-90 chars long ");
            }

            if (!addr.Contains("1"))
            {
                throw new Exception("Invalid bech32 string, must contain a seperator!");
            }

            var sep_pos = addr.LastIndexOf("1");

            //always cast to lower, if the checksum fails its invalid anyway
            Hrp = addr.Substring(0, sep_pos).ToLower();
            DataString = addr.Substring(sep_pos + 1, addr.Length - sep_pos - 1).ToLower();

            if (Hrp.Length == 0 || Hrp.Length > 83)
            {
                throw new Exception("Invalid bech32 string, hrp must be between 1-83 chars long");
            }

            if (DataString.Length < 6 || DataString.IndexOfAny(ExcludedDataChars) >= 0)
            {
                throw new Exception($"Invalid bech32 string, data must be greater than 6 chars long and not contain any of the following [{string.Join(',', ExcludedDataChars)}]");
            }

            var data = Bech32DecodeString(DataString);
            
            if (!VeryifyChecksum(Hrp, data))
            {
                throw new Exception("Invalid bech32 string, checksum failed");
            }

            //remove checksum 
            DataBytes = new int[data.Length - 6];
            Array.Copy(data, DataBytes, DataBytes.Length);
        }

        /// <summary>
        /// Encodes data into a bech32 string
        /// </summary>
        /// <param name="hrp"></param>
        /// <param name="data"></param>
        public Bech32(string hrp, int[] data)
        {
            Hrp = hrp;
            DataBytes = data;

            var checksum = CreateChecksum(hrp, data);
            var combined = data.Concat(checksum);

            var ds = new char[combined.Length];
            for (var x = 0; x < combined.Length; x++)
            {
                ds[x] = EncodingTable[combined[x]];
            }

            DataString = new string(ds);
            
            if (!VeryifyChecksum(hrp, combined))
            {
                throw new Exception("Invalid bech32 string, checksum failed");
            }
        }

        private int[] Bech32DecodeString(string str)
        {
            var ret = new int[str.Length];

            for (var x = 0; x < str.Length; x++)
            {
                ret[x] = DecodingTable[str[x]];
            }

            return ret;
        }

        private bool VeryifyChecksum(string hrp, int[] bytes)
        {
            var hrp_exp = ExpandHrp(hrp);

            var poly = hrp_exp.Concat(bytes);
            return Polymod(poly) == 1;
        }

        private int[] CreateChecksum(string hrp, int[] data)
        {
            var hrp_ext = ExpandHrp(hrp);
            var values = hrp_ext.Concat(data).Concat(new int[6]);
            var mod = Polymod(values) ^ 1;

            var ret = new int[6];
            for (var x = 0; x < 6; x++)
            {
                ret[x] = (mod >> 5 * (5 - x)) & 31;
            }
            return ret;
        }

        private int[] ExpandHrp(string hrp)
        {
            var ret = new int[(hrp.Length * 2) + 1];

            for (var x = 0; x < hrp.Length; x++)
            {
                ret[x] = hrp[x] >> 5;
            }

            ret[hrp.Length] = 0;

            for (var x = 0; x < hrp.Length; x++)
            {
                ret[hrp.Length + 1 + x] = hrp[x] & 31;
            }

            return ret;
        }

        private int Polymod(int[] values)
        {
            var chk = 1;
            for (var p = 0; p < values.Length; ++p)
            {
                var top = chk >> 25;
                chk = (chk & 0x1ffffff) << 5 ^ values[p];
                for (var i = 0; i < 5; ++i)
                {
                    if (((top >> i) & 1) != 0)
                    {
                        chk ^= Generator[i];
                    }
                }
            }
            return chk;
        }

        public override string ToString()
        {
            return $"{Hrp}1{DataString}";
        }
    }
}
