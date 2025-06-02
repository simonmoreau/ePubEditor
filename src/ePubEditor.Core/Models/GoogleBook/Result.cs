using System.Text.Json.Serialization;

namespace ePubEditor.Core.Models.GoogleBook
{
    public class Result
    {
        [JsonPropertyName("kind")]
        public string Kind { get; set; }

        [JsonPropertyName("totalItems")]
        public int? TotalItems { get; set; }

        [JsonPropertyName("items")]
        public List<Item> Items { get; set; }
    }


}
