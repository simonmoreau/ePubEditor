using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ePubEditor.Core.Models
{
    public class Settings
    {
        public AzureOpenAI AzureOpenAI { get; set; } = new AzureOpenAI();
        /// <summary>
        /// Gets or sets the local directory for Sarcina file operations.
        /// </summary>
        public string LocalDirectory { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the Gemini API key for external integrations.
        /// </summary>
        public string GeminiApiKey { get; set; } = string.Empty;
        public string ComicVineApi { get; set; } = string.Empty;
    }
}
