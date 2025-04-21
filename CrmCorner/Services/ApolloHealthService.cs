using System.Net.Http.Headers;
using System.Text.Json;

namespace CrmCorner.Services
{
    public class ApolloHealthService
    {
        private readonly HttpClient _httpClient;

        public ApolloHealthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApolloHealthResult> CheckHealthAsync(string apiKey)
        {
            var url = "https://api.apollo.io/v1/auth/health";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-api-key", apiKey);
            request.Headers.Add("Cache-Control", "no-cache");

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            return new ApolloHealthResult
            {
                Healthy = response.IsSuccessStatusCode,
                Message = content
            };
        }


    }

    public class ApolloHealthResult
    {
        public bool Healthy { get; set; }
        public bool IsLoggedIn { get; set; }
        public string? Error { get; set; }
        public string Message { get; set; }  // 👈 Bu satırı ekle
    }
}
