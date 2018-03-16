using hashstream.bitcoin_lib.BlockChain;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace hashstream.bitcoin_lib.P2P
{

    public class Alert
    {

    }

    public class FeeFilter
    {
        public UInt64 FeeRate { get; set; }
    }

    public class FilterAdd
    {
        public UInt64 Size { get; set; }
        public byte[] Element { get; set; }
    }

    public class FilterClear
    {

    }

    public class FilterLoad
    {
        public UInt64 FilterBytes { get; set; }
        public byte[] Filter { get; set; }
        public UInt32 HashFunctions { get; set; }
        public UInt32 Tweak { get; set; }
        public byte Flags { get; set; }
    }

    public class GetAddr
    {

    }

    public class Reject
    {
        public UInt64 MessageLength { get; set; }
        public string Message { get; set; }
        public RejectCode Code { get; set; }
        public UInt64 ReasonLength { get; set; }
        public string Reason { get; set; }
        public byte[] Extra { get; set; }
    }

    public enum RejectCode
    {
        DecodeError = 0x01,
        InvalidData = 0x10,
        Outdated = 0x11,
        Duplicate = 0x12,
        InvalidTx = 0x40,
        DustyTx = 0x41,
        LowFeeTx = 0x42,
        InvalidChain = 0x43
    }

    public class SendHeaders
    {

    }
    

    

    [Flags]
    public enum Services
    {
        Unknown = 0x00,
        NODE_NETWORK = 0x01
    }
}