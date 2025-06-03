using EpubCore;
using EpubCore.Format;
using ePubEditor.Core.Models.GoogleBook;
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
        public string FilePath { get; set; } 


        public string Author { get { return string.Join(", ", Authors);  } }
        public string Tag { get { return string.Join(", ", Tags); } }
        public string Language { get { return string.Join(", ", Languages); } }

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
            foreach (string author in Authors)
            {
                writer.AddAuthor(author);
            }

            
            if (!string.IsNullOrWhiteSpace(Publisher))
            {
                writer.ClearPublishers();
                writer.AddPublisher(Publisher);
            }

            foreach (string language in Languages)
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

        public string WriteMetadata()
        {
            string separator = ";";
            // Helper to join lists as comma-separated values
            string JoinList(IEnumerable<string> list) => list != null ? string.Join(",", list) : "";

            // Format date as ISO 8601 or empty
            string published = Published.HasValue ? Published.Value.ToString("o", CultureInfo.InvariantCulture) : "";

            // Prepare fields in order
            string[] fields = new[]
            {
                Title ?? "",
                JoinList(Authors),
                Publisher ?? "",
                JoinList(Tags),
                JoinList(Languages),
                published,
                GoogleIdentifier ?? "",
                IsbnIdentifier ?? "",
                Description ?? "",
                CoverImagePath ?? ""
            };

            // Escape separator in fields if needed (basic CSV escaping)
            string Escape(string s) =>
                s.Contains(separator) || s.Contains("\"") || s.Contains("\n")
                    ? $"\"{s.Replace("\"", "\"\"")}\""
                    : s;

            return string.Join(separator, fields.Select(Escape));
        }

        public static BookMetadata EmptyMetadata(string epubFile)
        {
            BookMetadata metadata = new BookMetadata();

            metadata.FilePath = epubFile;

            return metadata;
        }
        public static BookMetadata FromEpubFile(string epubFile)
        {
            BookMetadata metadata = new BookMetadata();

            // Read metadata
            EpubBook book = EpubReader.Read(epubFile);
            metadata.Title = book.Title;
            metadata.Authors.AddRange(book.Authors);
            metadata.Publisher = string.Join(", ", book.Publishers);
            metadata.Tags.AddRange(book.Format.Opf.Metadata.Subjects);
            metadata.Languages.AddRange(book.Format.Opf.Metadata.Languages);
            DateTime date = new DateTime();
            DateTime.TryParse(book.Format.Opf.Metadata.Dates.Select(d => d.Text).ToList().FirstOrDefault(),out date);
            metadata.Published = date;


            string title = book.UniqueIdentifier;

            EpubCore.Format.OpfMetadataIdentifier? identifier = book.Format.Opf.Metadata.Identifiers
                .Where(i => i.Text.Length == 13 || i.Text.Length == 10).FirstOrDefault();

            if (identifier != null)
            {
                metadata.IsbnIdentifier = identifier.Text;
            }

            metadata.Description = string.Join(", ", book.Format.Opf.Metadata.Descriptions); 
            metadata.FilePath = epubFile;

            return metadata;
        }
        public static BookMetadata FromGoogleResult(Item item, string? isbn = null)
        {
            VolumeInfo info = item.VolumeInfo;
            if (info == null) throw new ArgumentException("VolumeInfo is null in the provided Google Book item.");

            BookMetadata metadata = new BookMetadata
            {
                Title = info.Title,
                Authors = info.Authors ?? new List<string>(),
                Publisher = info.Publisher,
                Tags = info.Categories ?? new List<string>(),
                Languages = !string.IsNullOrWhiteSpace(info.Language) ? new List<string> { info.Language } : new List<string>(),
                Description = info.Description,
                GoogleIdentifier = item.Id,
                CoverImagePath = info.ImageLinks?.Thumbnail
            };

            // Parse published date (can be yyyy-MM-dd, yyyy-MM, or yyyy)
            if (!string.IsNullOrWhiteSpace(info.PublishedDate))
            {
                DateTime published;
                if (DateTime.TryParseExact(info.PublishedDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out published) ||
                    DateTime.TryParseExact(info.PublishedDate, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out published) ||
                    DateTime.TryParseExact(info.PublishedDate, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out published))
                {
                    metadata.Published = published;
                }
            }

            if (!string.IsNullOrEmpty(isbn))
            {
                metadata.IsbnIdentifier = isbn;
            }
            else
            {
                // Try to get ISBN_13 first, then ISBN_10 if not found
                string? isbn13 = info.IndustryIdentifiers?
                    .FirstOrDefault(id => id.Type == "ISBN_13")?.Identifier;
                string? isbn10 = info.IndustryIdentifiers?
                    .FirstOrDefault(id => id.Type == "ISBN_10")?.Identifier;

                metadata.IsbnIdentifier = isbn13 ?? isbn10;
            }
            

            return metadata;
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
