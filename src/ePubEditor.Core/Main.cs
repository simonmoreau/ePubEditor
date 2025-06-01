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
                // Example: extract title from file name (customize as needed)
                string title = Path.GetFileNameWithoutExtension(epubFile);
                if (title.Contains(" - "))
                {
                    title = title.Substring(0, title.IndexOf(" - "));
                }
                string author = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(epubFile));
                // Prepare the process start info
                var psi = new ProcessStartInfo
                {
                    FileName = "fetch-ebook-metadata",
                    Arguments = $"--title \"{title}\" --authors \"{author}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = psi })
                {
                    process.Start();
                    string output = await process.StandardOutput.ReadToEndAsync();
                    BookMetadata bookMetadata = BookMetadata.FromCliOutput(output);

                    string error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    // Handle output/error as needed
                    Console.WriteLine($"Output for '{title}':\n{output}");
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        Console.WriteLine($"Error for '{title}':\n{error}");
                    }
                }

                break;
            }
        }

    }
}
