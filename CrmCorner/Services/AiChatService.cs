using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CrmCorner.Services
{
    public class AiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AiChatService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<string>> GenerateReplySuggestionsAsync(List<string> messages)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            var model = _configuration["OpenAI:Model"] ?? "gpt-4o-mini";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return new List<string>
                {
                    "Merhaba, nasıl yardımcı olabilirim?",
                    "Detay paylaşabilir misin?",
                    "Tamamdır, kontrol edip dönüş yaparım."
                };
            }

            var conversationText = messages == null || !messages.Any()
                ? "Henüz mesaj yok."
                : string.Join("\n", messages);

            var prompt =
$@"Aşağıdaki şirket içi mesajlaşmaya göre kullanıcıya 3 kısa cevap önerisi üret.
Kurallar:
- Çok kısa olsun
- Profesyonel ama doğal olsun
- Her satırda 1 öneri ver
- Numaralandırma yapma

Konuşma:
{conversationText}";

            var requestBody = new
            {
                model = model,
                input = prompt
            };

            var json = JsonSerializer.Serialize(requestBody);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new List<string>
                {
                    "Tamamdır 👍",
                    "Kontrol edip dönüş yaparım",
                    "Detay verebilir misin?"
                };
            }

            using var document = JsonDocument.Parse(responseContent);

            string outputText = "";

            if (document.RootElement.TryGetProperty("output", out var outputArray))
            {
                foreach (var item in outputArray.EnumerateArray())
                {
                    if (item.TryGetProperty("content", out var contentArray))
                    {
                        foreach (var content in contentArray.EnumerateArray())
                        {
                            if (content.TryGetProperty("text", out var textElement))
                            {
                                outputText += textElement.GetString() + "\n";
                            }
                        }
                    }
                }
            }

            var suggestions = outputText
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Take(3)
                .ToList();

            if (!suggestions.Any())
            {
                suggestions = new List<string>
                {
                    "Tamamdır 👍",
                    "Kontrol ediyorum",
                    "Biraz daha detay verebilir misin?"
                };
            }

            return suggestions;
        }
    }
}