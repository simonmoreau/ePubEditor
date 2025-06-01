using EpubCore;
using EpubCore.Format;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ePubEditor.Core
{
    internal class BookMetadata
    {
        public string Title { get; set; }
        public List<string> Authors { get; set; } = new List<string>();
        public string Publisher { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> Languages { get; set; } = new List<string>();
        public DateTime? Published { get; set; }
        public string GoogleIdentifier { get; set; }
        public string IsbnIdentifier { get; set; }
        public string Description { get; set; }
        public string CoverImagePath { get; set; } // Path to cover image file

        // Private constructor
        private BookMetadata() { }

        public void WriteMetadata(string epubFile)
        {
            // Read an epub file
            EpubBook book = EpubReader.Read(epubFile);
            book.Format.Opf.Metadata.Dates.Clear();

            if (!string.IsNullOrWhiteSpace(Description))
            {
                book.Format.Opf.Metadata.Descriptions.Clear();
                book.Format.Opf.Metadata.Descriptions.Add(Description);
            }

            EpubWriter writer = new EpubWriter(book);

            writer.SetTitle(Title); 

            writer.ClearAuthors();
            foreach (var author in Authors)
            {
                writer.AddAuthor(author);
            }

            
            if (!string.IsNullOrWhiteSpace(Publisher))
            {
                writer.ClearPublishers();
                writer.AddPublisher(Publisher);
            }

            foreach (var language in Languages)
            {
                writer.AddLanguage(language);
            }

            if (Published.HasValue)
            {
                writer.SetDate(Published.Value);
            }

            if (!string.IsNullOrWhiteSpace(GoogleIdentifier))
            {
                writer.AddIdentifier("GOOGLE", GoogleIdentifier);
            }

            if (!string.IsNullOrWhiteSpace(IsbnIdentifier))
            {
                writer.AddISBN(IsbnIdentifier);
            }


            string authorNames = string.Join(", ", Authors);
            string outputDir = @$"C:\Users\smoreau\Downloads\Output\{authorNames}\{Title}.epub";


            // Ensure the output directory exists
            string outputDirectory = Path.GetDirectoryName(outputDir);
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            writer.Write(outputDir);

        }

        // Static factory method
        public static BookMetadata FromCliOutput(string output)
        {
            BookMetadata metadata = new BookMetadata();
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                int idx = line.IndexOf(':');
                if (idx < 0) continue;
                string key = line.Substring(0, idx).Trim();
                string value = line.Substring(idx + 1).Trim();

                switch (key)
                {
                    case "Title":
                        metadata.Title = value;
                        break;
                    case "Author(s)":
                        metadata.Authors = value.Split(',').Select(a => a.Trim()).Where(a => !string.IsNullOrEmpty(a)).ToList();
                        break;
                    case "Publisher":
                        metadata.Publisher = value;
                        break;
                    case "Tags":
                        metadata.Tags = value.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();
                        break;
                    case "Languages":
                        metadata.Languages = value.Split(',').Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToList();
                        break;
                    case "Published":
                        if (DateTime.TryParse(value, null, DateTimeStyles.RoundtripKind, out DateTime dt))
                            metadata.Published = dt;
                        break;
                    case "Identifiers":
                        // Example: google:4vN4DwAAQBAJ, isbn:9781537802534
                        IEnumerable<string> ids = value.Split(',').Select(i => i.Trim());
                        foreach (string? id in ids)
                        {
                            if (id.StartsWith("google:"))
                                metadata.GoogleIdentifier = id.Substring("google:".Length);
                            else if (id.StartsWith("isbn:"))
                                metadata.IsbnIdentifier = id.Substring("isbn:".Length);
                        }
                        break;
                    case "Comments":
                        metadata.Description = value;
                        break;
                }
            }

            return metadata;
        }
    }
}
