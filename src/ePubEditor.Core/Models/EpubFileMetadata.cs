using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ePubEditor.Core.Models
{
    internal class EpubFileMetadata
    {
        public string? Title { get; set; }
        public string? Publisher { get; set; }
        public string? Published { get; set; }
        public string? GoogleIdentifier { get; set; }
        public string? IsbnIdentifier { get; set; }
        public string? Description { get; set; }
        public string? CoverImagePath { get; set; }
        public string? FilePath { get; set; }
        public string? Author { get; set; }
        public string? Tag { get; set; }
        public string? Language { get; set; }
        public string? Second_Alternate_Author { get; set; }
        public string? Alternate_Author { get; set; }
        public string? Alternate_Title { get; set; }
    }
}
