using hashstream.bitcoin_lib.Encoding;
using System;

namespace hashstream.bitcoin_lib.BlockChain
{
    public enum Base58AddressType
    {
        Unknown,
        PubKey,
        Script,
        SecretKey,
        ExtPubKey,
        ExtSecretKey
    }

    public class Base58Address : Address
    {
        public Base58AddressType AddressType { get; internal set; }

        public Base58Address(byte[] bytes)
        {
            AddressBytes = bytes;

            var at = GetNetworkAndType();
            Network = at.Item1;
            AddressType = at.Item2;
        }

        public Base58Address(AddressNetwork net, Base58AddressType addr_type, byte[] bytes)
        {
            Network = net;
            AddressType = addr_type;
            AddressBytes = bytes;
        }

        public Base58Address(string addr)
        {
            Network = AddressNetwork.Unknown;

            var db = Base58.Decode(addr);
            if (!Base58.ValidateChecksum(db))
            {
                throw new Exception("Checksum failed");
            }

            AddressBytes = db;

            var t = GetNetworkAndType();
            Network = t.Item1;
            AddressType = t.Item2;
        }

        private Tuple<AddressNetwork, Base58AddressType> GetNetworkAndType()
        {
            //Main Net
            if(AddressBytes[0] == 0)
            {
                return Tuple.Create(AddressNetwork.Main, Base58AddressType.PubKey);
            } 
            else if(AddressBytes[0] == 5)
            {
                return Tuple.Create(AddressNetwork.Main, Base58AddressType.Script);
            }
            else if(AddressBytes[0] == 128)
            {
                return Tuple.Create(AddressNetwork.Main, Base58AddressType.SecretKey);
            }
            else if(AddressBytes[0] == 0x04 && AddressBytes[1] == 0x88)
            {
                if(AddressBytes[2] == 0xb2 && AddressBytes[3] == 0x1e)
                {
                    return Tuple.Create(AddressNetwork.Main, Base58AddressType.ExtPubKey);
                }
                else if(AddressBytes[2] == 0xad && AddressBytes[3] == 0xe4)
                {
                    return Tuple.Create(AddressNetwork.Main, Base58AddressType.ExtSecretKey);
                }
            }

            //test net
            if (AddressBytes[0] == 111)
            {
                return Tuple.Create(AddressNetwork.Test, Base58AddressType.PubKey);
            }
            else if (AddressBytes[0] == 196)
            {
                return Tuple.Create(AddressNetwork.Test, Base58AddressType.Script);
            }
            else if (AddressBytes[0] == 238)
            {
                return Tuple.Create(AddressNetwork.Test, Base58AddressType.SecretKey);
            }
            else if (AddressBytes[0] == 0x04 && AddressBytes[1] == 0x35)
            {
                if (AddressBytes[2] == 0x87 && AddressBytes[3] == 0xcf)
                {
                    return Tuple.Create(AddressNetwork.Test, Base58AddressType.ExtPubKey);
                }
                else if (AddressBytes[2] == 0x83 && AddressBytes[3] == 0x94)
                {
                    return Tuple.Create(AddressNetwork.Test, Base58AddressType.ExtSecretKey);
                }
            }

            return Tuple.Create(AddressNetwork.Unknown, Base58AddressType.Unknown);
        }

        private byte[] GetPrefix()
        {
            switch (Network)
            {
                case AddressNetwork.Main:
                    {
                        switch (AddressType)
                        {
                            case Base58AddressType.PubKey: return new byte[] { 0 };
                            case Base58AddressType.Script: return new byte[] { 5 };
                            case Base58AddressType.SecretKey: return new byte[] { 128 };
                            case Base58AddressType.ExtPubKey: return new byte[] { 0x04, 0x88, 0xb2, 0x1e };
                            case Base58AddressType.ExtSecretKey: return new byte[] { 0x04, 0x88, 0xad, 0xe4 };
                        }
                        break;
                    }
                case AddressNetwork.RegTest:
                case AddressNetwork.Test:
                    {
                        switch (AddressType)
                        {
                            case Base58AddressType.PubKey: return new byte[] { 111 };
                            case Base58AddressType.Script: return new byte[] { 196 };
                            case Base58AddressType.SecretKey: return new byte[] { 239 };
                            case Base58AddressType.ExtPubKey: return new byte[] { 0x04, 0x35, 0x87, 0xcf };
                            case Base58AddressType.ExtSecretKey: return new byte[] { 0x04, 0x35, 0x83, 0x94 };
                        }
                        break;
                    }
            }

            return new byte[0];
        }

        public override string ToString()
        {
            var prefix = GetPrefix();

            var ab = prefix.Concat(AddressBytes);
            var chk = ab.SHA256d();
            var ab_final = new byte[ab.Length + 4];
            Array.Copy(ab, 0, ab_final, 0, ab.Length);
            Array.Copy(chk, 0, ab_final, ab.Length, 4);
            
            return Base58.Encode(ab_final);
        }
        
        public override bool Equals(object obj)
        {
            if(obj is Base58Address)
            {
                var b = (Base58Address)obj;
                var tm = Network == b.Network && AddressType == b.AddressType;
                if (AddressBytes.Length == b.AddressBytes.Length)
                {
                    for (var z = 0; z < AddressBytes.Length; z++)
                    {
                        if (b.AddressBytes[z] != AddressBytes[z])
                        {
                            return false;
                        }
                    }
                }else
                {
                    return false;
                }

                return tm;
            }

            return false;
        }
        
        public override byte[] ToArray()
        {
            //remove address type and checksum
            var ab = new byte[AddressBytes.Length - 5];
            Array.Copy(AddressBytes, 1, ab, 0, ab.Length);

            return ab;
        }

        public override int GetHashCode()
        {
            return -1698289053 + AddressType.GetHashCode();
        }
    }
}
