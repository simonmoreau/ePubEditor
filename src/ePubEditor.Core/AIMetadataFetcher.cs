using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ePubEditor.Core
{
    internal class AIMetadataFetcher
    {
        private readonly IChatClient _chatClient;

        public AIMetadataFetcher(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task FetchMetadata()
        {

            ChatOptions _chatOptions = new ChatOptions();

            List<Microsoft.Extensions.AI.ChatMessage> conversation = [];
            conversation.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, "Hello"));
            Microsoft.Extensions.AI.ChatCompletion response = await _chatClient.CompleteAsync(conversation, _chatOptions);

            conversation.Add(response.Message);
        }
    }
}
