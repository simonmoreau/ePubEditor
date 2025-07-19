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
        public ComicVineWriter() { }

        public async Task RunComicVineWriter()
        {
            string filePath = @"C:\Users\smoreau\Desktop\BD\0_Day\Aberzen\Aberzen #01 - 03.cbz";
            string series = "Aberzen";
            string publisher = "Soleil";
            string year = "2001";

            try
            {
                string result = await CallComicTaggerCLI(filePath, series, publisher, year);
                Console.WriteLine($"ComicVineWriter executed successfully: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing ComicVineWriter: {ex.Message}");
            }
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

            var outputCompletionSource = new TaskCompletionSource<string>();
            var errorCompletionSource = new TaskCompletionSource<string>();

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
