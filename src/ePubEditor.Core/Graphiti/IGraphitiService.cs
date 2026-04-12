namespace ePubEditor.Core.Graphiti
{
    internal interface IGraphitiService
    {
        Task<Response> CreateEpisodes(string group_id, List<Episode> messages);
    }
}