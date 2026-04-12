using ePubEditor.Core.Models.GoogleBook;
using ePubEditor.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ePubEditor.Core.Graphiti
{
    internal class GraphitiService : IGraphitiService
    {
        private readonly HttpClient _httpClient;

        public GraphitiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Response> CreateEpisodes(string group_id, List<Episode> episodes)
        {
            string uri = $"episodes";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);

            var addCommentRequest = new { group_id, episodes };

            string serializedCommentToCreate = JsonSerializer.Serialize(addCommentRequest);

            request.Content = new StringContent(serializedCommentToCreate);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            Response result = await RequestSender.SendRequest<Response>(request, _httpClient, CancellationToken.None);

            return result;
        }
    }
}
