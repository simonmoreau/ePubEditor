using System.Text.Json.Serialization;

namespace ePubEditor.Core.Models.GoogleBook
{
    public class Pdf
    {
        [JsonPropertyName("isAvailable")]
        public bool? IsAvailable { get; set; }

        [JsonPropertyName("acsTokenLink")]
        public string AcsTokenLink { get; set; }
    }


}
