using System.Text.Json.Serialization;

namespace ePubEditor.Core.Models.GoogleBook
{
    public class SaleInfo
    {
        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("saleability")]
        public string Saleability { get; set; }

        [JsonPropertyName("isEbook")]
        public bool? IsEbook { get; set; }
    }


}
