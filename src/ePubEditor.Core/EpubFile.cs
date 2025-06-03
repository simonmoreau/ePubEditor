using EpubCore;
using ePubEditor.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace ePubEditor.Core
{
    internal class EpubFile
    {
        public static EpubFile FromEpubMetadata(string epubFile)
        {
            // Read an epub file
            EpubBook book = EpubReader.Read(epubFile);

            // Read metadata
            string title = book.Title;
            List<string> authors = book.Authors.ToList();
            string? author = authors.Count > 0 ? authors[0] : null;

            

            if (string.IsNullOrEmpty(title))
            {
                title = GetTitleFromFileName(epubFile);
            }

            if (string.IsNullOrEmpty(author))
            {
                author = GetAuthorFromFileName(epubFile);
            }

            return new EpubFile(title, author); 
        }

        public static EpubFile FromFilePath(string epubFile)
        {
            string title = GetTitleFromFileName(epubFile);
            string author = GetAuthorFromFileName(epubFile);

            return new EpubFile(title, author);
        }

        public static EpubFile FromMetadata(InitialMetadata metadata)
        {
            string title = metadata.Title;
            string author = metadata.Authors;

            return new EpubFile(title, author);
        }

        private static string GetAuthorFromFileName(string epubFile)
        {
            return Path.GetFileNameWithoutExtension(Path.GetDirectoryName(epubFile));
        }

        private static string GetTitleFromFileName(string epubFile)
        {
            // Example: extract title from file name (customize as needed)
            string title = Path.GetFileNameWithoutExtension(epubFile);
            if (title.Contains(" - "))
            {
                title = title.Substring(0, title.IndexOf(" - "));
            }

            return title;
        }

        private EpubFile(string title, string author)
        {
            Title = title;
            Author = author;
        }

        public string Title { get; set; }
        public string Author { get; set; }
    }
}
