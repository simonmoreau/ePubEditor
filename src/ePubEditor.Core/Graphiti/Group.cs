using System.Text.Json.Serialization;

namespace ePubEditor.Core.Graphiti
{
    public class Group
    {
        [JsonConstructor]
        public Group(
            string groupId,
            List<Message> messages
        )
        {
            this.GroupId = groupId;
            this.Messages = messages;
        }

        [JsonPropertyName("group_id")]
        public string GroupId { get; }

        [JsonPropertyName("messages")]
        public IReadOnlyList<Message> Messages { get; }
    }
}
