using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ePubEditor.Core.Graphiti
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class Message
    {
        [JsonConstructor]
        public Message(
            string content,
            string? uuid,
            string name,
            string roleType,
            string role,
            DateTime timestamp,
            string sourceDescription
        )
        {
            this.Content = content;
            this.Uuid = uuid;
            this.Name = name;
            this.RoleType = roleType;
            this.Role = role;
            this.Timestamp = timestamp;
            this.SourceDescription = sourceDescription;
        }

        [JsonPropertyName("content")]
        public string Content { get; }

        [JsonPropertyName("uuid")]
        public string? Uuid { get; }

        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("role_type")]
        public string RoleType { get; }

        [JsonPropertyName("role")]
        public string Role { get; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; }

        [JsonPropertyName("source_description")]
        public string SourceDescription { get; }
    }


}
