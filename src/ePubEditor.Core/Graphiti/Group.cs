using System.Text.Json.Serialization;

namespace ePubEditor.Core.Graphiti
{
    public class Group
    {
        [JsonConstructor]
        public Group(
            string groupId,
            List<Episode> episodes
        )
        {
            this.GroupId = groupId;
            this.Episodes = episodes;
        }

        [JsonPropertyName("group_id")]
        public string GroupId { get; }

        [JsonPropertyName("episodes")]
        public IReadOnlyList<Episode> Episodes { get; }
    }
}
