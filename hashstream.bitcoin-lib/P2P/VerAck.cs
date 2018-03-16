namespace hashstream.bitcoin_lib.P2P
{
    public class VerAck : IStreamable, ICommand
    {
        public string Command => "verack";

        public void ReadFromPayload(byte[] data, int offset) { }

        public byte[] ToArray()
        {
            return new byte[0];
        }
    }
}
