using ePubEditor.Core;
using ePubEditor.Core.Graphiti;

namespace ePubEditor.Test;

public class GraphitiImporterTest
{
    private readonly Main _main;

    public GraphitiImporterTest()
    {
        _main = new Main();
    }

    [Fact]
    public async Task Import()
    {
        GraphitiImporter importer = _main.GetService<GraphitiImporter>();
        await importer.ImportData();
    }

    [Fact]
    public async Task ImportData_ReadsMarkdownFilesAndCreatesMessages()
    {
        string originalDirectory = Directory.GetCurrentDirectory();
        string temporaryDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("D"));
        string resourcesDirectory = Path.Combine(temporaryDirectory, "Ressources");

        Directory.CreateDirectory(resourcesDirectory);
        await File.WriteAllTextAsync(Path.Combine(resourcesDirectory, "first.md"), "# First\nContent one");
        await File.WriteAllTextAsync(Path.Combine(resourcesDirectory, "second.md"), "# Second\nContent two");

        try
        {
            Directory.SetCurrentDirectory(temporaryDirectory);

            FakeGraphitiService graphitiService = new FakeGraphitiService();
            GraphitiImporter importer = new GraphitiImporter(graphitiService);

            await importer.ImportData();

            Assert.Equal("Ressources", graphitiService.GroupId);
            Assert.NotNull(graphitiService.Messages);
            Assert.Equal(2, graphitiService.Messages.Count);
            Assert.Contains(graphitiService.Messages, message => message.Content.Contains("Content one", StringComparison.Ordinal));
            Assert.Contains(graphitiService.Messages, message => message.Content.Contains("Content two", StringComparison.Ordinal));
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
            if (Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(temporaryDirectory, true);
            }
        }
    }

    private sealed class FakeGraphitiService : IGraphitiService
    {
        public string? GroupId { get; private set; }

        public List<Episode> Messages { get; private set; } = new List<Episode>();

        public Task<Response> CreateEpisodes(string group_id, List<Episode> messages)
        {
            GroupId = group_id;
            Messages = messages;

            Response response = new Response("ok", true);
            return Task.FromResult(response);
        }
    }
}
