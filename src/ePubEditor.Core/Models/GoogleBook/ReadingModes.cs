using System.Text.Json.Serialization;

namespace ePubEditor.Core.Models.GoogleBook
{
    public class ReadingModes
    {
        [JsonPropertyName("text")]
        public bool? Text { get; set; }

        [JsonPropertyName("image")]
        public bool? Image { get; set; }
    }


}
