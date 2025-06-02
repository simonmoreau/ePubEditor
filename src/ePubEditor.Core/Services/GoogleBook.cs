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
        private int _requestCount = 0;
        private DateTime _lastResetTime = DateTime.MinValue;
        private const int RateLimit = 90; // Set a rate limit slightly lower than 100 to be safe
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public GoogleBook(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private async Task EnsureRateLimitAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                if (_lastResetTime != DateTime.Today)
                {
                    // Reset the counter if it's a new day
                    _requestCount = 0;
                    _lastResetTime = DateTime.Today;
                }

                if (_requestCount >= RateLimit)
                {
                    TimeSpan timeUntilNextMinute = TimeSpan.FromMinutes(1);
                    await Task.Delay(timeUntilNextMinute);
                    _requestCount = 0; // Reset after waiting
                }

                _requestCount++;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<Result> GetBookInfoAsync(string insb)
        {
            await EnsureRateLimitAsync();
            string uri = $"volumes?q=isbn:{insb}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);

            Result result = await RequestSender.SendRequest<Result>(request, _httpClient, CancellationToken.None);

            return result;
        }

        public async Task<Result> GetBookInfoAsync(string title, string author)
        {
            await EnsureRateLimitAsync();
            string encodeAuthor = Uri.EscapeDataString(author.Replace(" ", "+"));
            string encodeTitle = Uri.EscapeDataString(title.Replace(" ", "+"));
            string uri = $"volumes?q=inauthor:\"{encodeAuthor}\"intitle:\"{encodeTitle}\"";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);

            Result result = await RequestSender.SendRequest<Result>(request, _httpClient, CancellationToken.None);

            return result;
        }

        public async Task<Result> GetBookInfoFromTitleAsync(string title)
        {
            await EnsureRateLimitAsync();

            string encodeTitle = Uri.EscapeDataString(title.Replace(" ", "+"));
            string uri = $"volumes?q=intitle:\"{encodeTitle}\"";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);

            Result result = await RequestSender.SendRequest<Result>(request, _httpClient, CancellationToken.None);

            return result;
        }
    }
}
