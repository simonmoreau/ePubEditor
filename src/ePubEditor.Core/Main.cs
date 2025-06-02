using ePubEditor.Core.Services;
using FuzzySharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ePubEditor.Core
{
    public class Main
    {
        private readonly ServiceProvider _serviceProvider;

        public Main()
        {
            _serviceProvider = ConfigureServices(new ServiceCollection());
        }

        private ServiceProvider ConfigureServices(IServiceCollection services)
        {
            string baseUrl = "https://www.googleapis.com/books/v1/";
            services.AddHttpClient<IGoogleBook, GoogleBook>
                (client => client.BaseAddress = new Uri(baseUrl));

            return services.BuildServiceProvider();
        }

        public async Task Start()
        {
            // Get all epub in the current directory
            List<InitialMetadata> initialMetadata = Helper.LoadObjectsFromCSV<InitialMetadata>("inputs");

            IGoogleBook googleBook = _serviceProvider.GetRequiredService<IGoogleBook>();
            string outputPath = "C:\\Users\\smoreau\\Downloads\\Output\\output.csv";
            using (StreamWriter writer = new StreamWriter(outputPath, append: true))
            {
                foreach (InitialMetadata initialLine in initialMetadata)
                {
                    Debug.WriteLine(initialLine.Uuid);

                    try
                    {
                        BookMetadata? metadata = null;
                        if (!string.IsNullOrWhiteSpace(initialLine.Isbn))
                        {
                            Models.GoogleBook.Result result = await googleBook.GetBookInfoAsync(initialLine.Isbn);
                            if (result?.Items != null && result.Items.Count > 0)
                            {
                                metadata = BookMetadata.FromGoogleResult(result.Items[0], initialLine.Isbn);
                            }
                        }

                        if (metadata == null)
                        {
                            Models.GoogleBook.Result result = await googleBook.GetBookInfoAsync(initialLine.Title,initialLine.Authors);
                            if (result?.Items != null && result.Items.Count > 0)
                            {
                                foreach (Models.GoogleBook.Item item in result?.Items)
                                {
                                    BookMetadata tempmetadata = BookMetadata.FromGoogleResult(item, initialLine.Isbn);
                                    if (tempmetadata.Title == null) continue; // Skip if title is null
                                    int titleScore = Fuzz.Ratio(tempmetadata.Title, initialLine.Title);
                                    if (titleScore < 80) continue;
                                    if (tempmetadata.Authors == null) continue; // Skip if authors are null
                                    int authorScore = Fuzz.Ratio(string.Join(", ", tempmetadata.Authors), initialLine.Authors);
                                    if (authorScore < 80) continue;
                                    metadata = tempmetadata;
                                    break;
                                }
                            }
                        }

                        if (metadata == null)
                        {
                            Models.GoogleBook.Result result = await googleBook.GetBookInfoFromTitleAsync(initialLine.Title);
                            if (result?.Items != null && result.Items.Count > 0)
                            {
                                foreach (Models.GoogleBook.Item item in result?.Items)
                                {
                                    BookMetadata tempmetadata = BookMetadata.FromGoogleResult(item, initialLine.Isbn);
                                    if (tempmetadata.Title == null) continue; // Skip if title is null
                                    int titleScore = Fuzz.Ratio(tempmetadata.Title, initialLine.Title);
                                    if (titleScore < 80) continue;
                                    metadata = tempmetadata;
                                    break;
                                }
                            }
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
            using (System.Diagnostics.Process process = new System.Diagnostics.Process { StartInfo = psi })
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
