using System.Threading.Tasks;

namespace hashstream.nodes
{
    class Program
    {
        static Task Main(string[] args)
        {
            var ns = new NodeScraper();
            return ns.Run();
        }
    }
}
