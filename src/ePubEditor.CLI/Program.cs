using System;
using System.Threading.Tasks;
using ePubEditor.Core;
using Microsoft.Extensions.DependencyInjection;

namespace ePubEditor.CLI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            ServiceProvider serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            Main main = new Main();

            await main.Start();

            
        }
    }
}