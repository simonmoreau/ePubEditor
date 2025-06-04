
using CsvHelper;
using ePubEditor.Core.Models;
using ePubEditor.Core.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace ePubEditor.Core
{
    internal class AIMetadataFetcher
    {
        private readonly ChatClient _chatClient;
        private readonly GenerativeModel _generativeModel;
        private readonly List<BookMetadata> _completedBookMetadata = new List<BookMetadata>();
        private readonly object _completedBookMetadataLock = new object();


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

            await CommpleteMetadata(epubFileMetadata.Take(5).ToList());

        }

        private async Task CommpleteMetadata(List<EpubFileMetadata> epubFileMetadata)
        {

            JsonSerializerOptions options = new()
            {
                TypeInfoResolver = new DefaultJsonTypeInfoResolver
                {
                    Modifiers = { PromptPreparationModifier }
                }
            };

            string fileMetadata = JsonSerializer.Serialize(epubFileMetadata, options);

            // Prepare the prompt with the metadata of the epub files
            string prompt = "Here is a list of epub file matadata in json format :" +
                    $"{fileMetadata}" +
                    "These metadata contains Alternate_Author, Second_Alternate_Author and Alternate_Title which may or may not be accurate but can help you identify the book " +
                    "Please complete each book metadata as best as you can by using" +
                    "both the text provided along with your own knowledge about these book." +
                    "If you know that the book is part of a series, please add the series to the result." +
                    "If the language is 'UND', please replace it with your own knowledge about the book.";

            GenerationConfig generationConfig = new GenerationConfig()
            {
                ResponseMimeType = "application/json",
                ResponseSchema = new List<OutputMetadata>()
            };

            CountTokensResponse tokenCount = await _generativeModel.CountTokens(prompt);

            GenerateContentResponse response = await _generativeModel.GenerateContent(prompt, generationConfig = generationConfig);

            JsonSerializerOptions deserializeOptions = new()
            {
                PropertyNameCaseInsensitive = true
            };

            List<OutputMetadata> outputMetadata = JsonSerializer.Deserialize<List<OutputMetadata>>(response.Text, deserializeOptions);

            List<BookMetadata> completedMetadata = new List<BookMetadata>();

            for (int i = 0; i < outputMetadata.Count; i++)
            {
                EpubFileMetadata epubFileMetadata1 = epubFileMetadata[i];

                BookMetadata bookMetadata = BookMetadata.EmptyMetadata(epubFileMetadata1.FilePath);
                bookMetadata.Title = outputMetadata[i].Title ?? epubFileMetadata1.Title;
                bookMetadata.Authors.Add(outputMetadata[i].Authors);
                bookMetadata.Publisher = outputMetadata[i].Publisher ?? epubFileMetadata1.Publisher;
                bookMetadata.Tags.Add(outputMetadata[i].Tags);
                bookMetadata.Languages.Add(outputMetadata[i].Languages);

                if (outputMetadata[i].PublicationYear != 0)
                {
                    bookMetadata.Published = new DateTime(outputMetadata[i].PublicationYear, 1, 1);
                }

                bookMetadata.IsbnIdentifier = epubFileMetadata1.IsbnIdentifier;
                bookMetadata.GoogleIdentifier = epubFileMetadata1.GoogleIdentifier;
                bookMetadata.Description = outputMetadata[i].Description ?? epubFileMetadata1.Description;

                completedMetadata.Add(bookMetadata);
            }

            lock (_completedBookMetadataLock)
            {
                _completedBookMetadata.AddRange(completedMetadata);
            }

        }

        private void PromptPreparationModifier(JsonTypeInfo typeInfo)
        {
            if (typeInfo.Type != typeof(EpubFileMetadata))
                return;

            string[] excludedProperties = [
                nameof(EpubFileMetadata.GoogleIdentifier),
                nameof(EpubFileMetadata.Published),
                nameof(EpubFileMetadata.CoverImagePath),
                nameof(EpubFileMetadata.FilePath)
            ];

            foreach (JsonPropertyInfo prop in typeInfo.Properties)
            {
                if (excludedProperties.Contains(prop.Name))
                {
                    // Set ShouldSerialize to always return false for the 'Published' property
                    prop.ShouldSerialize = (context, val) => false;
                }

                if (prop.Name == nameof(EpubFileMetadata.Language))
                {
                    // Set ShouldSerialize to always return false for the 'Author' property
                    prop.ShouldSerialize = (context, val) =>
                    {
                        if (val is string str && str == "UND")
                        {
                            return false; // Do not serialize if the language is 'UND'
                        }
                        return true; // Otherwise, serialize normally
                    };
                }
            }
        }

    }
}
