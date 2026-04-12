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
                string resourcesDirectory = @"C:\Users\smoreau\Documents\Obsidian\Notes\EAI\Arlanxeo";

                if (!Directory.Exists(resourcesDirectory))
                {
                    Console.WriteLine($"Graphiti import directory not found: {resourcesDirectory}");
                    return;
                }

                string[] markdownFiles = Directory.GetFiles(resourcesDirectory, "*.md", SearchOption.AllDirectories);
                List<Episode> episodes = new List<Episode>();

                foreach (string markdownFile in markdownFiles)
                {
                    string content = await File.ReadAllTextAsync(markdownFile);
                    string name = Path.GetFileNameWithoutExtension(markdownFile);
                    DateTime timestamp = File.GetLastWriteTimeUtc(markdownFile);
                    string obsidanPath = markdownFile.Replace(@"C:\Users\smoreau\Documents\Obsidian\Notes\", "");
                    string obsidianLink = $"obsidian://open?vault=Notes&file={Uri.EscapeDataString(obsidanPath)}";

                    Episode episode = new Episode(
                        "WebPermis Project :" + content,
                        null,
                        name,
                        timestamp,
                        obsidianLink
                    );

                    episodes.Add(episode);
                }

                if (episodes.Count == 0)
                {
                    Console.WriteLine($"No markdown files were found in {resourcesDirectory}.");
                    return;
                }

                string groupId = "main";
                await _graphitiService.CreateEpisodes(groupId, episodes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing data to Graphiti: {ex.Message}");
            }
        }

    }
}
