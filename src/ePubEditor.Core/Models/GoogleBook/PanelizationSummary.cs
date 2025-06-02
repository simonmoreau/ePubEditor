using System.Text.Json.Serialization;

namespace ePubEditor.Core.Models.GoogleBook
{
    public class PanelizationSummary
    {
        [JsonPropertyName("containsEpubBubbles")]
        public bool? ContainsEpubBubbles { get; set; }

        [JsonPropertyName("containsImageBubbles")]
        public bool? ContainsImageBubbles { get; set; }
    }


}
