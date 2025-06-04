
using CsvHelper;
using ePubEditor.Core.Models;
using ePubEditor.Core.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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
            List<EpubFileMetadata> epubFileMetadata = await Helper.LoadObjectFromJson<List<EpubFileMetadata>>("epub_files");

            string fileMetadata = JsonSerializer.Serialize(epubFileMetadata.Take(5));

        // Prepare the prompt with the metadata of the epub files
        string prompt = "Here is a list of epub file matadata in json format :" +
                $"{fileMetadata}" +
                "These metadata contains Alternate_Author, Second_Alternate_Author and Alternate_Title which may or may not be accurate but can help you identify the book " +
                "Please complete each book metadata as best as you can by using" +
                "both the text provided along with your own knowledge about these book." +
                "If you know that the book is part of a series, please add the series to the result";

            var generationConfig = new GenerationConfig()
            {
                ResponseMimeType = "application/json",
                ResponseSchema = new List<OutputMetadata>()
            };

            GenerateContentResponse response = await _generativeModel.GenerateContent(prompt, generationConfig = generationConfig);

            string result = response.Text;
        }
    }
}
