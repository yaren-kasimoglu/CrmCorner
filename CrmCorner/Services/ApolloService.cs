using CrmCorner.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CrmCorner.Services
{
    public class ApolloService
    {
        private readonly HttpClient _httpClient;

        public ApolloService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ApolloUserDto>> GetContactsAsync(string apiKey) // direkt kullanıcının bilgisini çekiyo örn : yaren kasimoglu
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://api.apollo.io/v1/users/search"),
                Headers =
        {
            { "accept", "application/json" },
            { "Cache-Control", "no-cache" },
            { "x-api-key", apiKey },
        },
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<JsonElement>(body);

                var users = json.GetProperty("users").EnumerateArray().Select(user => new ApolloUserDto
                {
                    FirstName = user.GetProperty("first_name").GetString(),
                    LastName = user.GetProperty("last_name").GetString(),
                    Email = user.GetProperty("email").GetString(),
                    Title = user.GetProperty("title").GetString(),
                    CreatedAt = user.TryGetProperty("created_at", out var created)
                        ? DateTime.Parse(created.GetString())
                        : DateTime.MinValue
                }).ToList();

                return users;
            }
        }
    }



    public class ApolloUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
