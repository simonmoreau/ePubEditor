using ComicVineApi;
using ComicVineApi.Clients;
using ComicVineApi.Models;
using ePubEditor.Core.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ePubEditor.Core.Comics
{
    public class ComicVineWriter
    {
        private readonly Settings _settings;

        public ComicVineWriter(IOptions<Settings> options)
        {
            _settings = options.Value;
        }

        public async Task RunComicVineWriter()
        {
            string filePath = @"C:\Users\smoreau\Desktop\BD\0_Day\Aberzen\Aberzen #01 - 03.cbz";
            string series = "Aberzen";
            string publisher = "Soleil";
            string year = "2001";

            try
            {
                //string result = await CallComicTaggerCLI(filePath, series, publisher, year);
                await CallComicVineAPI();
                // Console.WriteLine($"ComicVineWriter executed successfully: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing ComicVineWriter: {ex.Message}");
            }
        }

        private async Task CallComicVineAPI()
        {
            // create the client
            ComicVineClient client = new ComicVineClient(_settings.ComicVineApi, "comic-tagger");

            // search for a comic book
            Filter<Series, ISeriesSortable, ISeriesFilterable> payload = client.Series.Filter()
                        .WithValue(x => x.Name, "Les aventures extraordinaires d'Adèle Blanc-Sec");

            //var series = await client.Series.GetAsync(51404);
            var volume = await client.Volume.GetAsync(51404);

            List<Series> page = await payload.ToListAsync();
        }

        private async Task<string> CallComicTaggerCLI(string filePath, string series, string publisher, string year)
        {
            string command = $"comictagger";
            string[] cmdArguments = {
                "-s -t cr -o -v",
                $"--metadata \"series: {series}, year: {year}, publisher: {publisher}\"",
                $"\"{filePath}\""
            };

            Process process = new Process();
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = EscapeCommandLineArguments(cmdArguments);
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            TaskCompletionSource<string> outputCompletionSource = new TaskCompletionSource<string>();
            TaskCompletionSource<string> errorCompletionSource = new TaskCompletionSource<string>();

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    // errorCompletionSource.SetResult(e.Data);
                    Console.WriteLine($"Error: {e.Data}");
                }
            };

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputCompletionSource.SetResult(e.Data);
                    Console.WriteLine($"Error: {e.Data}");
                }

            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            string[] result = await Task.WhenAll(outputCompletionSource.Task, errorCompletionSource.Task);

            process.WaitForExit();

            return result.First();
        }

        private string EscapeCommandLineArguments(string[] args)
        {
            string arguments = "";
            foreach (string arg in args)
            {
                arguments += " " +
                    arg.Replace("\\", "\\").Replace("\"", "\"") +
                    "";
            }
            return arguments;
        }

    }
}
