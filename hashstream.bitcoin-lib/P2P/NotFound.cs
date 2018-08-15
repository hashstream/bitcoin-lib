namespace hashstream.bitcoin_lib.P2P
{
    public class NotFound : Inv, ICommand
    {
        public new string Command => "notfound";
    }
}
