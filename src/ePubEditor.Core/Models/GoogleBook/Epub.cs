using System.Text.Json.Serialization;

namespace ePubEditor.Core.Models.GoogleBook
{
    public class Epub
    {
        [JsonPropertyName("isAvailable")]
        public bool? IsAvailable { get; set; }
    }


}
