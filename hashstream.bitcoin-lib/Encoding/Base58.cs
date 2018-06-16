using System;
using System.Linq;
using System.Numerics;
using System.Text;

namespace hashstream.bitcoin_lib.Encoding
{
    public class Base58
    {
        private const string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        public static string Encode(byte[] input)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(input);
            }
            var bi = new BigInteger(input);

            var s = new StringBuilder();
            while (bi > 0)
            {
                var mod = bi % 58;
                s.Insert(0, new[] { Alphabet[(int)mod] });
                bi /= 58;
            }

            s.Insert(0, new[] { Alphabet[(int)bi] });

            for(var x = input.Length - 1; x > 0 ; x--)
            {
                var z = input[x];
                if (z == 0)
                {
                    s.Insert(0, new[] { Alphabet[0] });
                }
                else
                {
                    break;
                }
            }
            return s.ToString();
        }

        public static byte[] Decode(string input)
        {
            var bytes = DecodeToBigInteger(input).ToByteArray();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            // We may have got one more byte than we wanted, if the high bit of the next-to-last byte was not zero. This
            // is because BigIntegers are represented with twos-compliment notation, thus if the high bit of the last
            // byte happens to be 1 another 8 zero bits will be added to ensure the number parses as positive. Detect
            // that case here and chop it off.
            var stripSignByte = bytes.Length > 1 && bytes[0] == 0 && bytes[1] >= 0x80;
            // Count the leading zeros, if any.
            var leadingZeros = 0;
            for (var i = 0; input[i] == Alphabet[0]; i++)
            {
                leadingZeros++;
            }
            // Now cut/pad correctly. Java 6 has a convenience for this, but Android can't use it.
            var tmp = new byte[bytes.Length - (stripSignByte ? 1 : 0) + leadingZeros];
            Array.Copy(bytes, stripSignByte ? 1 : 0, tmp, leadingZeros, tmp.Length - leadingZeros);
            return tmp;
        }
        
        public static BigInteger DecodeToBigInteger(string input)
        {
            var bi = new BigInteger(0);
            // Work backwards through the string.
            for (var i = input.Length - 1; i >= 0; i--)
            {
                var alphaIndex = Alphabet.IndexOf(input[i]);
                if (alphaIndex == -1)
                {
                    throw new Exception("Illegal character " + input[i] + " at " + i);
                }
                bi = BigInteger.Add(bi, BigInteger.Multiply(new BigInteger(alphaIndex), BigInteger.Pow(new BigInteger(58), input.Length - 1 - i)));
            }
            return bi;
        }

        public static bool ValidateChecksum(byte[] data)
        {
            var db = new byte[data.Length - 4];
            var chk = new byte[4];

            Array.Copy(data, 0, db, 0, db.Length);
            Array.Copy(data, data.Length - 4, chk, 0, chk.Length);

            var n_chk = db.SHA256d();

            return chk[0] == n_chk[0] 
                && chk[1] == n_chk[1] 
                && chk[2] == n_chk[2]
                && chk[3] == n_chk[3];
        }
    }
}