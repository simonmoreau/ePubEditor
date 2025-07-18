using ePubEditor.Core;
using ePubEditor.Core.Comics;

namespace ePubEditor.Test
{
    public class MainTest
    {
        private readonly Main _main;

        public MainTest()
        {
            _main = new Main();
        }

        [Fact]
        public async Task Start()
        {
            Main main = new Main();
            await main.Start();
        }

        [Fact]
        public async Task RenameComics()
        {
            ComicsRenamer comicsRenamer = _main.GetService<ComicsRenamer>();
            string directoryPath = "C:\\Users\\smoreau\\Desktop\\BD\\BD Library\\0_Day"; // Adjust the path as necessary
            await comicsRenamer.RenameComics(directoryPath);
        }
    }
}