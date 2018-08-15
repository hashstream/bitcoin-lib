namespace hashstream.bitcoin_lib.P2P
{
    public class Pong : Ping, ICommand
    {
        public new string Command => "pong";
    }
}
