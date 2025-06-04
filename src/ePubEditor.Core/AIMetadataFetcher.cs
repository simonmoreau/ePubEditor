
using Microsoft.Extensions.AI;
using Mscc.GenerativeAI;
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
        private readonly GenerativeModel _generativeModel;

        public AIMetadataFetcher(ChatClient chatClient, GenerativeModel generativeModel)
        {
            _chatClient = chatClient;
            _generativeModel = generativeModel;
        }

        public async Task FetchMetadataWithOpenAI()
        {

            ChatOptions _chatOptions = new ChatOptions();

            List<Microsoft.Extensions.AI.ChatMessage> conversation = [];
            conversation.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, "Hello"));

            ChatCompletion completion = _chatClient.CompleteChat([
                        new UserChatMessage("Hi, can you help me?")
                    ]);

            string result = completion.Content.First().Text;
        }

        public async Task FetchMetadataWithGemini()
        {
            string prompt = "Hello";

            GenerateContentResponse response = _generativeModel.GenerateContent(prompt).Result;

            string result = response.Text;
        }
    }
}
