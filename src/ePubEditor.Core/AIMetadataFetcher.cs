
using Azure;
using Azure.AI.OpenAI;
using CsvHelper;
using CsvHelper.Configuration;
using ePubEditor.Core.Models;
using ePubEditor.Core.Services;
using Json.Schema;
using Json.Schema.Generation.Generators;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace ePubEditor.Core
{
    internal class AIMetadataFetcher
    {
        private readonly ChatClient _chatClient;
        private readonly Gemini _gemeniSettings;
        private readonly List<BookMetadata> _completedBookMetadata = new List<BookMetadata>();
        private readonly object _completedBookMetadataLock = new object();
        private readonly string _outputPath = "C:\\Users\\smoreau\\Downloads\\Output\\output_gemini.csv";

        public AIMetadataFetcher(ChatClient chatClient, IOptions<Gemini> gemeniSettings)
        {
            _chatClient = chatClient;
            _gemeniSettings = gemeniSettings.Value;
        }

        public async Task FetchMetadataWithOpenAI()
        {
            await FetchMetadata(GetMetadataFromOpenAI);
        }

        private async Task<List<OutputMetadata>> GetMetadataFromOpenAI(string prompt)
        {
            // Create a JSON schema for the CalendarEvent structured response
            JsonSerializerOptions options = JsonSerializerOptions.Default;

            JsonSchemaExporterOptions exporterOptions = new()
            {
                TreatNullObliviousAsNonNullable = true,
            };

            JsonNode jsonSchema = options.GetJsonSchemaAsNode(typeof(OutputMetadataList), exporterOptions);

            ChatCompletionOptions chatCompletionOptions = new ChatCompletionOptions()
            {
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                        nameof(OutputMetadataList),
                        BinaryData.FromString(jsonSchema.ToString()))
            };

            ChatCompletion completion = await _chatClient.CompleteChatAsync(
                [new UserChatMessage(prompt)],
                chatCompletionOptions);

            string sanitizeText = completion.Content.First().Text.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("\t", "");

            if (sanitizeText == null)
            {
                return new List<OutputMetadata>();
            }

            JsonSerializerOptions deserializeOptions = new()
            {
                PropertyNameCaseInsensitive = true
            };

            OutputMetadataList outputsMetadata = JsonSerializer.Deserialize<OutputMetadataList>(sanitizeText, deserializeOptions);
            return outputsMetadata.OutputMetadatas;

        }

        public async Task FetchMetadataWithGemini()
        {
            await FetchMetadata(GetMetadataFromGemeni);
        }

        public async Task FetchMetadata(Func<string, Task<List<OutputMetadata>>> getMetadata)
        {
            List<EpubFileMetadata> epubFileMetadata = await Helper.LoadObjectFromJson<List<EpubFileMetadata>>("epub_files");

            const int batchSize = 15;
            const int maxCallsPerMinute = 1;
            const int delayBetweenCallsMs = 60000; // 60,000 ms  = 1 m

            List<List<EpubFileMetadata>> batches = epubFileMetadata.Take(batchSize* maxCallsPerMinute)
                .Select((item, index) => new { item, index })
                .GroupBy(x => x.index / (batchSize * maxCallsPerMinute))
                .Select(g => g.Select(x => x.item).ToList())
                .ToList();

            int i = 1;
            foreach (List<EpubFileMetadata>? batch in batches)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                Console.WriteLine($"Starting {i}/{batches.Count}");


                List<List<EpubFileMetadata>> subatches = batch
                    .Select((item, index) => new { item, index })
                    .GroupBy(x => x.index / (batchSize ))
                    .Select(g => g.Select(x => x.item).ToList())
                    .ToList();

                List<Task> tasks = new List<Task>();
                foreach (List<EpubFileMetadata> subatch in subatches)
                {
                    tasks.Add(CommpleteMetadata(subatch, getMetadata));
                }

                Task runAllRequest = Task.WhenAll(tasks);
                Task delayTask = Task.Delay(delayBetweenCallsMs);

                await Task.WhenAll(runAllRequest, delayTask);

                stopwatch.Stop();
                Console.WriteLine($"{stopwatch.Elapsed} - End {i}/{batches.Count}");
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

        private async Task CommpleteMetadata(List<EpubFileMetadata> epubFilesMetadata, Func<string, Task<List<OutputMetadata>>> getMetadata)
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

            Console.WriteLine(prompt);


                List<OutputMetadata> outputsMetadata = await getMetadata(prompt);


            List<BookMetadata> completedMetadata = new List<BookMetadata>();

            for (int i = 0; i < outputsMetadata.Count; i++)
            {
                EpubFileMetadata epubFileMetadata = epubFilesMetadata[i];
                OutputMetadata outputMetadata = outputsMetadata[i];

                BookMetadata bookMetadata = BookMetadata.EmptyMetadata(epubFileMetadata.FilePath);

                bookMetadata.Title = SelectValue(outputMetadata.Title, epubFileMetadata.Title);

                bookMetadata.Authors.Add(outputMetadata.Author);

                bookMetadata.Publisher = epubFileMetadata.Publisher;
                bookMetadata.Tags.Add(epubFileMetadata.Tag);
                bookMetadata.Languages.Add(outputMetadata.Language);
                bookMetadata.Series = outputMetadata.Series;

                if (outputMetadata.PublicationYear != 0)
                {
                    bookMetadata.Published = new DateTime(outputMetadata.PublicationYear, 1, 1);
                }

                bookMetadata.IsbnIdentifier = epubFileMetadata.IsbnIdentifier;
                bookMetadata.GoogleIdentifier = epubFileMetadata.GoogleIdentifier;

                bookMetadata.Description = SelectValue(epubFileMetadata.Description, outputMetadata.Description).Replace("SUMMARY:", "");


                completedMetadata.Add(bookMetadata);
            }

            lock (_completedBookMetadataLock)
            {
                _completedBookMetadata.AddRange(completedMetadata);
            }

        }

        private async Task<List<OutputMetadata>> GetMetadataFromGemeni(string prompt)
        {
            GenerativeModel model = new GenerativeModel()
            {
                ApiKey = _gemeniSettings.Key,
                Model = Model.Gemini15Flash,
            };

            GenerationConfig generationConfig = new GenerationConfig()
            {
                ResponseMimeType = "application/json",
                ResponseSchema = new List<OutputMetadata>()
            };

            //CountTokensResponse tokenCount = await _generativeModel.CountTokens(prompt);

            GenerateContentResponse response = await model.GenerateContent(prompt, generationConfig = generationConfig);



            Console.WriteLine("Response from Gemini: ");
            Console.WriteLine("--------------------------------------------------");
            Console.Write(response.Text);

            if (string.IsNullOrEmpty(response.Text))
            {
                Console.WriteLine("No response received from Gemini.");
                return null;
            }

            string sanitizeText = response.Text.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("\t", "");

            if (sanitizeText == null)
            {
                return new List<OutputMetadata>();
            }

            JsonSerializerOptions deserializeOptions = new()
            {
                PropertyNameCaseInsensitive = true
            };

            List<OutputMetadata> outputsMetadata = JsonSerializer.Deserialize<List<OutputMetadata>>(sanitizeText, deserializeOptions);
            return outputsMetadata;
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
