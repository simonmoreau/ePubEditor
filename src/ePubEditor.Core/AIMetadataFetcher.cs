
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
        private readonly ChatClient _chatClient;

        public AIMetadataFetcher(ChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task FetchMetadataWithOpenAI()
        {

            ChatOptions _chatOptions = new ChatOptions();

            List<Microsoft.Extensions.AI.ChatMessage> conversation = [];
            conversation.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, "Hello"));

            ChatCompletion completion = _chatClient.CompleteChat([
                        new UserChatMessage("Hi, can you help me?")
                    ]);

            var result = completion.Content.First().Text;
        }

        public async Task FetchMetadataWithGemini()
        {
            //IMessage reply = await _geminiAgent.SendAsync("Can you write a piece of C# code to calculate 100th of fibonacci?");

            //string result = reply.GetContent();
        }
    }
}
