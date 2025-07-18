using Json.Schema.Generation;
using System.Text.Json;
using Json.Schema.Generation.Generators;
using Microsoft.Extensions.AI;
using System;
using System.IO;
using System.Reflection.Emit;
using System.Text;
using Json.Schema;
using Json.More;


namespace ePubEditor.Core.Comics
{
    /// <summary>
    /// Renames comics using OpenAI service for intelligent suggestions.
    /// </summary>
    /// <param name="chatClient">Injected OpenAI chat client.</param>
    /// <example>
    /// <code>
    /// IChatClient chatClient = ...;
    /// ComicsRenamer renamer = new ComicsRenamer(chatClient);
    /// </code>
    /// </example>
    public class ComicsRenamer
    {
        private readonly IChatClient _chatClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComicsRenamer"/> class.
        /// </summary>
        /// <param name="chatClient">The OpenAI chat client to use for renaming operations.</param>
        public ComicsRenamer(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        /// <summary>
        /// For each subdirectory in the given directory, reads the prompt from the markdown file and sends it to the chat client.
        /// </summary>
        /// <param name="directoryPath">The path to the parent directory containing comic subdirectories.</param>
        /// <returns>The concatenated responses from the chat client for each subdirectory.</returns>
        /// <example>
        /// <code>
        /// string result = await renamer.RenameComics("C:\\Comics");
        /// </code>
        /// </example>
        public async Task RenameComics(string directoryPath)
        {
            if (directoryPath is null)
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
            }

            string[] subdirectories = Directory.GetDirectories(directoryPath);
            ChatOptions chatOptions = CreateChatOptions();

            foreach (string subdirectory in subdirectories)
            {
                string prompt = await GetPrompt(subdirectory);

                ChatResponse response = await _chatClient.GetResponseAsync(prompt, chatOptions);

                FileList? fileList = JsonSerializer.Deserialize<FileList>(response.Text);

                if (fileList == null || fileList.Files.Count == 0)
                {
                    throw new InvalidOperationException("No files found in the response.");
                }

                RenameFilesInDirectory(fileList, subdirectory);
            }


        }

        private ChatOptions CreateChatOptions()
        {
            JsonSchemaBuilder schemaBuilder = new JsonSchemaBuilder();
            JsonSchema schema = schemaBuilder.FromType<FileList>().Build();


            // To get the schema as a string:
            string schemaJson = schema.ToJsonDocument(new JsonSerializerOptions { WriteIndented = true }).ToString();


            ChatOptions chatOptions = new()
            {
                ResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat.ForJsonSchema(JsonSerializer.Deserialize<JsonElement>(schemaJson), "Schema")
            };

            return chatOptions;
        }

        private async Task<string> GetPrompt(string subdirectory)
        {
            string subdirectoryName = Path.GetFileName(subdirectory);
            string promptPath = $"{AppContext.BaseDirectory}\\Ressources\\renameComicsPrompt.md";

            if (!File.Exists(promptPath))
            {
                throw new FileNotFoundException($"Prompt file not found: {promptPath}");
            }

            string promptTemplate;
            using (FileStream stream = File.OpenRead(promptPath))
            using (StreamReader reader = new StreamReader(stream, Encoding.GetEncoding("iso-8859-1")))
            {
                promptTemplate = await reader.ReadToEndAsync();
            }

            string[] files = Directory.GetFiles(subdirectory, "*.cbz", SearchOption.TopDirectoryOnly);
            string filesNames = string.Join("\r\n", files.Select(f => Path.GetFileName(f)));

            promptTemplate = promptTemplate.Replace("{{directoryName}}", subdirectoryName);
            promptTemplate = promptTemplate.Replace("{{files}}", filesNames);
            return promptTemplate;
        }

        private void RenameFilesInDirectory(FileList fileList, string directory)
        {
            if (fileList is null)
            {
                throw new ArgumentNullException(nameof(fileList));
            }

            if (directory is null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            char[] invalidChars = Path.GetInvalidFileNameChars();

            foreach (FileRename fileRename in fileList.Files)
            {
                string sourcePath = Path.Combine(directory, fileRename.OldName);
                string destinationPath = Path.Combine(directory, fileRename.NewName);

                if (!File.Exists(sourcePath))
                {
                    continue;
                }

                if (File.Exists(destinationPath))
                {
                    continue;
                }

                if (fileRename.NewName.IndexOfAny(invalidChars) >= 0)
                {
                    throw new ArgumentException($"Destination file name contains invalid characters: {fileRename.NewName}");
                }

                File.Move(sourcePath, destinationPath);
            }
        }
    }

    public class FileList
    {
        public List<FileRename> Files { get; set; } = new List<FileRename>();
    }

    public class FileRename
    {
        public string OldName { get; set; } = string.Empty;
        public string NewName { get; set; } = string.Empty;
    }
}
