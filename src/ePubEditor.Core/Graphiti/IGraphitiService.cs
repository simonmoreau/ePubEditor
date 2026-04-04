namespace ePubEditor.Core.Graphiti
{
    internal interface IGraphitiService
    {
        Task<Response> CreateMessage(string group_id, List<Message> messages);
    }
}