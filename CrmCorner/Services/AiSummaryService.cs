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
    public class AiSummaryService : IAiSummaryService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenAISettings _settings;

        public AiSummaryService(
            HttpClient httpClient,
            IOptions<OpenAISettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<string> GenerateUserTaskSummaryCommentAsync(UserTaskSummaryDto summary)
        {
            var json = JsonSerializer.Serialize(summary, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var prompt = $@"
Sen profesyonel bir CRM satış analiz uzmanısın.

Aşağıdaki veriye göre Türkçe kısa ama akıllı yorum yap.

Kurallar:
- Veri uydurma
- Profesyonel ol
- 4-6 cümle yaz
- Güçlü ve zayıf noktaları söyle
- Kısa öneri ver

Veri:

{json}
";

            var payload = new
            {
                model = _settings.Model,
                messages = new[]
                {
                    new {
                        role = "user",
                        content = prompt
                    }
                },
                temperature = 0.7
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.openai.com/v1/chat/completions");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return "AI analizi alınamadı.";

            using var doc = JsonDocument.Parse(content);

            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString();
        }
    }
}