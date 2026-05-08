using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CrmCorner.Models.ChatCorner;
using CrmCorner.Models.Settings;
using Microsoft.Extensions.Options;

namespace CrmCorner.Services.ChatCorner
{
    public class OpenAiIntentParserService : IOpenAiIntentParserService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAISettings _settings;

        public OpenAiIntentParserService(
            HttpClient httpClient,
            IOptions<OpenAISettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<ChatIntentParseResultDto> ParseIntentAsync(string question)
        {

            Console.WriteLine("ApiKey: " + _settings.ApiKey);
            Console.WriteLine("Model: " + _settings.Model);
            Console.WriteLine("BaseUrl: " + _settings.BaseUrl);
            var endpoint = "https://api.openai.com/v1/chat/completions";

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            var prompt = $@"
Kullanıcının mesajını analiz et.
Sadece JSON dön.

Mesaj:
{question}

Intent seçenekleri:
UserTaskSummary
TopPerformer
PipelineSummary
MyAssignedTodos
Unknown

JSON format:
{{
 ""Intent"": ""..."",
 ""PeriodType"": ""this_month"",
 ""RawQuestion"": ""{question}""
}}";

            var body = new
            {
                model = _settings.Model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.1
            };

            var json = JsonSerializer.Serialize(body);

            var response = await _httpClient.PostAsync(
                endpoint,
                new StringContent(json, Encoding.UTF8, "application/json"));

            var result = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(result);

            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return JsonSerializer.Deserialize<ChatIntentParseResultDto>(
                content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
    }
}