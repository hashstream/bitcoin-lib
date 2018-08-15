namespace hashstream.bitcoin_lib.P2P
{
    public class GetData : Inv, ICommand
    {
        public new string Command => "getdata";
    }
}
