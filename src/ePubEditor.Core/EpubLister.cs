using CsvHelper;
using CsvHelper.Configuration;
using EpubCore;
using ePubEditor.Core.Services;
using FuzzySharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ePubEditor.Core
{
    internal class EpubLister
    {
        private readonly object _fileLock = new object();
        private readonly string _outputPath = "C:\\Users\\smoreau\\Downloads\\Output\\output_isbn.csv";
        private readonly List<BookMetadata> _books = new List<BookMetadata>();

        public EpubLister() { }

        public async Task ListEpub()
        {
            // Search for all .epub files, including subdirectories
            string directoryPath  = @"\\192.168.1.15\Drives\GoFlexDrive\eBook\epub";
            List<string> epubFiles = Directory.GetFiles(directoryPath, "*.epub", SearchOption.AllDirectories).ToList();

            List<Task> tasks = new List<Task>();
            foreach (string? epubFile in epubFiles)
            {
                tasks.Add(Task.Run(() => GetMetadata(epubFile)));
            }

            await Task.WhenAll(tasks);

            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ";"
            };

            using (var writer = new StreamWriter(_outputPath))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(_books);
            }
        }

        private void GetMetadata(string epubFile)
        {
            Debug.WriteLine(epubFile);

            try
            {
                BookMetadata bookMetadata = BookMetadata.FromEpubFile(epubFile);
                AddBookMetadata(bookMetadata);
            }
            catch (Exception ex)
            {
                BookMetadata bookMetadata = BookMetadata.EmptyMetadata(epubFile);
                AddBookMetadata(bookMetadata);

            }

            Debug.WriteLine("Done");
        }

        private void AddBookMetadata(BookMetadata bookMetadata)
        {
            lock (_fileLock)
            {
                _books.Add(bookMetadata);
                Debug.WriteLine(_books.Count);
            }
        }

        private void WriteLine(string line)
        {
            lock (_fileLock)
            {
                using (StreamWriter writer = new StreamWriter(_outputPath, append: true))
                {
                    writer.WriteLine(line);
                }
            }
        }
    }
}
