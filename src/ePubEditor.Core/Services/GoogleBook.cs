using ePubEditor.Core.Models.GoogleBook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ePubEditor.Core.Services
{
    internal class GoogleBook : IGoogleBook
    {
        private readonly HttpClient _httpClient;

        public GoogleBook(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Result> GetBookInfoAsync(string insb)
        {
            string uri = $"volumes?q=isbn:{insb}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);

            Result result = await RequestSender.SendRequest<Result>(request, _httpClient, CancellationToken.None);

            return result;
        }

        public async Task<Result> GetBookInfoAsync(string title, string author)
        {
            string encodeAuthor = Uri.EscapeDataString(author.Replace(" ","+"));
            string encodeTitle = Uri.EscapeDataString(title.Replace(" ", "+"));
            string uri = $"volumes?q=inauthor:\"{encodeAuthor}\"intitle:\"{encodeTitle}\"";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);

            Result result = await RequestSender.SendRequest<Result>(request, _httpClient, CancellationToken.None);

            return result;
        }

    }
}
