namespace hashstream.bitcoin_lib.P2P
{
    public interface IStreamable
    {
        string Command { get; }
        void ReadFromPayload(byte[] data, int offset);
        byte[] ToArray();
    }
}
