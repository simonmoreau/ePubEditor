using System.Text.Json.Serialization;

namespace ePubEditor.Core.Models.GoogleBook
{
    public class IndustryIdentifier
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }
    }


}
