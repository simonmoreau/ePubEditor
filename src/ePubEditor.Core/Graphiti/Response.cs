using System.Text.Json.Serialization;

namespace ePubEditor.Core.Graphiti
{
    public class Response
    {
        [JsonConstructor]
        public Response(
            string message,
            bool success
        )
        {
            this.Message = message;
            this.Success = success;
        }

        [JsonPropertyName("message")]
        public string Message { get; }

        [JsonPropertyName("success")]
        public bool Success { get; }
    }




}
