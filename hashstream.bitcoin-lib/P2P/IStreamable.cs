namespace hashstream.bitcoin_lib.P2P
{
    public interface IStreamable
    {
        void ReadFromPayload(byte[] data, int offset);
        byte[] ToArray();
    }
}
