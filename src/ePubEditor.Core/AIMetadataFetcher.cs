
using CsvHelper;
using CsvHelper.Configuration;
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
        private readonly string _outputPath = "C:\\Users\\smoreau\\Downloads\\Output\\output_gemini.csv";

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

            const int batchSize = 40;
            const int maxCallsPerMinute = 15;
            const int delayBetweenCallsMs = 3000; // 60,000 ms / 15 calls = 4,000 ms

            List<List<EpubFileMetadata>> batches = epubFileMetadata.Take(120)
                .Select((item, index) => new { item, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.item).ToList())
                .ToList();

            int i = 1;
            foreach (List<EpubFileMetadata>? batch in batches)
            {
                Console.WriteLine($"Starting {i}/{batches.Count}");
                Task metadataTask = CommpleteMetadata(batch);
                Task delayTask = Task.Delay(delayBetweenCallsMs);
                await Task.WhenAll(metadataTask, delayTask);
                Console.WriteLine($"End {i}/{batches.Count}");
                i++;
            }

            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ";"
            };

            using (StreamWriter writer = new StreamWriter(_outputPath))
            using (CsvWriter csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(_completedBookMetadata);
            }
        }

        private async Task CommpleteMetadata(List<EpubFileMetadata> epubFilesMetadata)
        {

            JsonSerializerOptions options = new()
            {
                TypeInfoResolver = new DefaultJsonTypeInfoResolver
                {
                    Modifiers = { PromptPreparationModifier }
                }
            };

            string fileMetadata = JsonSerializer.Serialize(epubFilesMetadata, options);

            // Prepare the prompt with the metadata of the epub files
            string prompt = "Here is a list of epub file matadata in json format :" +
                    $"{fileMetadata}" +
                    "These metadata contains Alternate_Author, Second_Alternate_Author and Alternate_Title which may or may not be accurate but can help you identify the book " +
                    "Please complete each book metadata as best as you can by using " +
                    "both the text provided along with your own knowledge about these book. " +
                    "If you know that the book is part of a series, please add the series to the result. " +
                    "If some data is missing from the metadata, complete it based on your own knowledge. " +
                    "If the language is 'UND', please replace it with your own knowledge about the book. ";

            GenerationConfig generationConfig = new GenerationConfig()
            {
                ResponseMimeType = "application/json",
                ResponseSchema = new List<OutputMetadata>()
            };

            //CountTokensResponse tokenCount = await _generativeModel.CountTokens(prompt);

            GenerateContentResponse response = await _generativeModel.GenerateContent(prompt, generationConfig = generationConfig);

            JsonSerializerOptions deserializeOptions = new()
            {
                PropertyNameCaseInsensitive = true
            };

            Console.Write(response.Text);
            List<OutputMetadata> outputsMetadata = JsonSerializer.Deserialize<List<OutputMetadata>>(response.Text, deserializeOptions);

            List<BookMetadata> completedMetadata = new List<BookMetadata>();

            for (int i = 0; i < outputsMetadata.Count; i++)
            {
                EpubFileMetadata epubFileMetadata = epubFilesMetadata[i];
                OutputMetadata outputMetadata = outputsMetadata[i];

                BookMetadata bookMetadata = BookMetadata.EmptyMetadata(epubFileMetadata.FilePath);

                bookMetadata.Title = SelectValue(outputMetadata.Title, epubFileMetadata.Title);

                bookMetadata.Authors.Add(outputsMetadata[i].Authors);

                bookMetadata.Publisher = SelectValue(outputMetadata.Publisher, epubFileMetadata.Publisher);
                bookMetadata.Tags.Add(outputsMetadata[i].Tags);
                bookMetadata.Languages.Add(outputsMetadata[i].Languages);
                bookMetadata.Series = outputsMetadata[i].Series;

                if (outputsMetadata[i].PublicationYear != 0)
                {
                    bookMetadata.Published = new DateTime(outputsMetadata[i].PublicationYear, 1, 1);
                }

                bookMetadata.IsbnIdentifier = epubFileMetadata.IsbnIdentifier;
                bookMetadata.GoogleIdentifier = epubFileMetadata.GoogleIdentifier;

                bookMetadata.Description = SelectValue(epubFileMetadata.Description, outputMetadata.Description)
                    .Replace("SUMMARY:", "")
                    .Replace('\n', ' ' )
                    .Replace('\r', ' ');

                completedMetadata.Add(bookMetadata);
            }

            lock (_completedBookMetadataLock)
            {
                _completedBookMetadata.AddRange(completedMetadata);
            }

        }

        private string SelectValue(string? firstValue, string? fallbackValue)
        {
            if (string.IsNullOrEmpty(firstValue))
            {
                if (string.IsNullOrEmpty(fallbackValue))
                {
                    return string.Empty;
                }
                else
                {
                    return fallbackValue;
                }
            }
            else
            {
                return firstValue;
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
