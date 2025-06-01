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
            List<InitialMetadata> initialMetadata = Helper.LoadObjectsFromCSV<InitialMetadata>("inputs");

            string outputPath = "C:\\Users\\smoreau\\Downloads\\Output\\output.csv";
            using (var writer = new StreamWriter(outputPath, append: true))
            {
                foreach (InitialMetadata initialLine in initialMetadata)
                {
                    Debug.WriteLine(initialLine.Uuid);
                    try
                    {
                        BookMetadata? metadata = null;
                        if (!string.IsNullOrWhiteSpace(initialLine.Isbn))
                        {
                            metadata = await FetchMetadata(initialLine.Isbn);
                        }

                        if (metadata == null)
                        {
                            EpubFile epub = EpubFile.FromMetadata(initialLine);
                            metadata = await FetchMetadata(epub);
                        }

                        if (metadata == null)
                        {
                            await writer.WriteLineAsync($"{initialLine.Uuid};;");
                        }
                        else
                        {
                            await writer.WriteLineAsync($"{initialLine.Uuid};{metadata.WriteMetadata()}");
                        }
                    }
                    catch (Exception ex)
                    {
                        await writer.WriteLineAsync($"{initialLine.Uuid};{ex.Message};");
                    }

                    await writer.FlushAsync();
                    Debug.WriteLine("Done");


                }
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

            return await ExecuteCommand(epub.Title, psi);
        }

        private async Task<BookMetadata?> FetchMetadata(string isbn)
        {
            // Prepare the process start info
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "fetch-ebook-metadata",
                Arguments = $"--isbn \"{isbn}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            return await ExecuteCommand(isbn, psi);
        }

        private static async Task<BookMetadata?> ExecuteCommand(string epub, ProcessStartInfo psi)
        {
            using (Process process = new Process { StartInfo = psi })
            {
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(output))
                {
                    Debug.WriteLine($"No metadata found for '{epub}'");
                    return null;
                }
                BookMetadata bookMetadata = BookMetadata.FromCliOutput(output);
                return bookMetadata;
            }
        }
    }
}
