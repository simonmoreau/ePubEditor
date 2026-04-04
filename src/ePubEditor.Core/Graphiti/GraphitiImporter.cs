using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ePubEditor.Test")]

namespace ePubEditor.Core.Graphiti
{
    internal class GraphitiImporter
    {
        private readonly IGraphitiService _graphitiService;

        public GraphitiImporter(IGraphitiService graphitiService)
        {
            _graphitiService = graphitiService;
        }

        public async Task ImportData()
        {
            try
            {
                string resourcesDirectory = @"C:\Users\smoreau\Documents\Obsidian\Notes\EAI\Eurotunnel";

                if (!Directory.Exists(resourcesDirectory))
                {
                    Console.WriteLine($"Graphiti import directory not found: {resourcesDirectory}");
                    return;
                }

                string[] markdownFiles = Directory.GetFiles(resourcesDirectory, "*.md", SearchOption.AllDirectories);
                List<Message> messages = new List<Message>();

                foreach (string markdownFile in markdownFiles)
                {
                    string content = await File.ReadAllTextAsync(markdownFile);
                    string name = Path.GetFileNameWithoutExtension(markdownFile);
                    DateTime timestamp = File.GetLastWriteTimeUtc(markdownFile);

                    Message message = new Message(
                        content,
                        null,
                        name,
                        "user",
                        "text",
                        timestamp,
                        "Meeting Note"
                    );

                    messages.Add(message);
                }

                if (messages.Count == 0)
                {
                    Console.WriteLine($"No markdown files were found in {resourcesDirectory}.");
                    return;
                }

                string groupId = Path.GetFileName(resourcesDirectory);
                await _graphitiService.CreateMessage(groupId, messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing data to Graphiti: {ex.Message}");
            }
        }

    }
}
