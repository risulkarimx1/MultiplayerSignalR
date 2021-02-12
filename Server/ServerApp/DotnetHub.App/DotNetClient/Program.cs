using System;
using System.Threading.Tasks;
using SigClient4;

namespace DotNetClient
{
    class Program
    {
        private static async Task Main(string[] args)
        {
            var sig4 = new SigClient4.SigClientContext();

            await sig4.CreateConnection().ConfigureAwait(false);
            sig4.MessageRec += FoundMesage;
            while (true)
            {
                var k = Console.ReadKey();
                if(k.Key == ConsoleKey.Escape) break;
                if (k.Key == ConsoleKey.A)
                {
                    Console.WriteLine(); 
                    await sig4.SendMessageAsync("risul", "says hi").ConfigureAwait(false);
                }
            }
        }

        private static void FoundMesage(object sender, MessageArgs e)
        {
            Console.WriteLine($"Found message {e.Name} {e.Message}");
        }

        private static void FoundMessage(string arg1, string arg2)
        {
            Console.WriteLine($"Message says {arg1}, {arg2}");
        }
    }
}