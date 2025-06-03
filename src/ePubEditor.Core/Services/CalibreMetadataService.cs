using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ePubEditor.Core.Services
{
    internal class CalibreMetadataService
    {
        public async Task<BookMetadata?> FetchMetadata(EpubFile epub)
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

        public async Task<BookMetadata?> FetchMetadata(string isbn)
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
