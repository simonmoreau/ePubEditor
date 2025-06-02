using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ePubEditor.Core.Services
{
    public static class RequestSender
    {
        public static async Task<T> SendRequest<T>(HttpRequestMessage request, HttpClient httpClient, CancellationToken cancellationToken)
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            cancellationToken.ThrowIfCancellationRequested();

            HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                Stream stream = await response.Content.ReadAsStreamAsync();

                if (!stream.CanRead)
                {
                    throw new Exception("Stream cannot be read");
                }

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<T>(stream, options);
            }

            HttpStatusCode statusCode = response.StatusCode;
            string errorResponseText = await response.Content.ReadAsStringAsync();

            if (statusCode == HttpStatusCode.Unauthorized || statusCode == HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException(errorResponseText);
            }
            else
            {
                throw new Exception(errorResponseText);
            }
        }

        public static async Task SendRequest(HttpRequestMessage request, HttpClient httpClient, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return;
            }

            HttpStatusCode statusCode = response.StatusCode;
            string errorResponseText = await response.Content.ReadAsStringAsync();

            if (statusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(errorResponseText);
            }
            else
            {
                throw new Exception(errorResponseText);
            }
        }
    }
}
