using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace ePubEditor.Core
{
    public class Main
    {
        public Main()
        {
            ServiceProvider serviceProvider =  new ServiceCollection()
                .BuildServiceProvider();
        }

        public async Task Start()
        {
            // Get all epub in the current directory
            string path = @"\\192.168.1.15\Drives\GoFlexDrive\eBook\epub\Z";

            List<string> epubFiles = Directory.GetFiles(path, "*.epub", SearchOption.AllDirectories).ToList();

            foreach (string epubFile in epubFiles)
            {
                Debug.WriteLine(epubFile);
                EpubFile epub = EpubFile.FromFilePath(epubFile);
                BookMetadata? metadata = await FetchMetadata(epub);

                if (metadata == null)
                {
                    epub = EpubFile.FromEpubMetadata(epubFile);
                    metadata = await FetchMetadata(epub);
                }

                if (metadata == null)
                {
                    continue;
                }

                metadata.WriteMetadata(epubFile);
                Debug.WriteLine("Done");


            }
        }

        private async Task<BookMetadata?> FetchMetadata(EpubFile epub)
        {
            // Prepare the process start info
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "fetch-ebook-metadata",
                Arguments = $"--title \"{epub.Title}\" --authors \"{epub.Author}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = psi })
            {
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(output))
                {
                    Console.WriteLine($"No metadata found for '{epub.Title}'");
                    return null;
                }
                BookMetadata bookMetadata = BookMetadata.FromCliOutput(output);
                return bookMetadata;

                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                // Handle output/error as needed
                Console.WriteLine($"Output for '{epub.Title}':\n{output}");
                if (!string.IsNullOrWhiteSpace(error))
                {
                    Console.WriteLine($"Error for '{epub.Title}':\n{error}");
                }
            }
        }
    }
}
