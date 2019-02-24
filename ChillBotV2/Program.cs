using System.Threading;
using System.Threading.Tasks;

namespace ChillBotV2
{
    internal class Program
    {
        // Create a safe way to cancel threads
        private static readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private static Task Main(string[] args)
            => new Core(_cts).InitialiseAsync();
    }
}
