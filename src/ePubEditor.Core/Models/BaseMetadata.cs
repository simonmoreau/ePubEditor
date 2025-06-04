using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ePubEditor.Core.Models
{
    internal class BaseMetadata
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
    }

    internal class OutputMetadata
    {
        public string Title { get; set; }
        public string Authors { get; set; }
        public string Publisher { get; set; }
        public string Series { get; set; }
        public string Tags { get; set; }
        public string Languages { get; set; }
        public string Published { get; set; }
        public string GoogleIdentifier { get; set; }
        public string IsbnIdentifier { get; set; }
        public string Description { get; set; }
    }
}
