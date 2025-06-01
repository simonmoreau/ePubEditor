using ePubEditor.Core;

namespace ePubEditor.Test
{
    public class MainTest
    {
        [Fact]
        public async Task Start()
        {
            Main main = new Main();
            await main.Start();
        }
    }
}