using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ePubEditor.Core
{
    internal class InitialMetadata
    {
        public string AuthorSort { get; set; }
        public string Authors { get; set; }
        public string Comments { get; set; }
        public string Cover { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Formats { get; set; }
        public string Isbn { get; set; }
        public string Id { get; set; }
        public string Identifiers { get; set; }
        public string Languages { get; set; }
        public string LibraryName { get; set; }
        public DateTime? Pubdate { get; set; }
        public string Publisher { get; set; }
        public double? Rating { get; set; }
        public string Series { get; set; }
        public double? SeriesIndex { get; set; }
        public long? Size { get; set; }
        public string Tags { get; set; }
        public string Title { get; set; }
        public string TitleSort { get; set; }
        public string Uuid { get; set; }
    }

    internal sealed class InitialMetadataMap : ClassMap<InitialMetadata>
    {
        public InitialMetadataMap()
        {
            Map(m => m.AuthorSort).Name("author_sort");
            Map(m => m.Authors).Name("authors");
            Map(m => m.Comments).Name("comments");
            Map(m => m.Cover).Name("cover");
            Map(m => m.Timestamp).Name("timestamp");
            Map(m => m.Formats).Name("formats");
            Map(m => m.Isbn).Name("isbn");
            Map(m => m.Id).Name("id");
            Map(m => m.Identifiers).Name("identifiers");
            Map(m => m.Languages).Name("languages");
            Map(m => m.LibraryName).Name("library_name");
            Map(m => m.Pubdate).Name("pubdate");
            Map(m => m.Publisher).Name("publisher");
            Map(m => m.Rating).Name("rating");
            Map(m => m.Series).Name("series");
            Map(m => m.SeriesIndex).Name("series_index");
            Map(m => m.Size).Name("size");
            Map(m => m.Tags).Name("tags");
            Map(m => m.Title).Name("title");
            Map(m => m.TitleSort).Name("title_sort");
            Map(m => m.Uuid).Name("uuid");
        }
    }
}
