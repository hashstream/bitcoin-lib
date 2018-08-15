namespace hashstream.bitcoin_lib.P2P
{
    public class GetHeaders : GetBlocks, ICommand
    {
        public new string Command => "getheaders";
    }
}
