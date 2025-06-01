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

            var main = serviceProvider.GetRequiredService<Main>();

            await main.Start();
        }
    }
}