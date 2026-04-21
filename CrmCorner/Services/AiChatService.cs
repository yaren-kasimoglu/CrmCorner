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
            try
            {
                var apiKey = _configuration["OpenAI:ApiKey"];
                var model = _configuration["OpenAI:Model"] ?? "gpt-4o-mini";

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    Console.WriteLine("OpenAI API key boş geliyor.");
                    return GetDefaultSuggestions();
                }

                var cleanedMessages = messages?
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .TakeLast(10)
                    .ToList() ?? new List<string>();

                var conversationText = cleanedMessages.Any()
                    ? string.Join("\n", cleanedMessages)
                    : "Henüz mesaj yok.";

                var prompt =
$@"Sen şirket içi yazışmalar için kısa cevap önerileri üreten bir asistansın.

Görev:
Aşağıdaki konuşmaya göre son mesaja uygun 3 farklı kısa cevap önerisi üret.

Kurallar:
- Türkçe yaz
- Maksimum 2-6 kelime olsun
- Profesyonel ama doğal olsun
- Birbirinden farklı olsun
- Çok genel ve sürekli tekrar eden cevaplar verme
- Numaralandırma yapma
- Açıklama ekleme
- Sadece 3 satır cevap ver

Konuşma:
{conversationText}";

                var requestBody = new
                {
                    model = model,
                    input = prompt
                };

                var json = JsonSerializer.Serialize(requestBody);

                Console.WriteLine("OpenAI request json:");
                Console.WriteLine(json);

                using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine("OpenAI status: " + response.StatusCode);
                Console.WriteLine("OpenAI response:");
                Console.WriteLine(responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    return GetDefaultSuggestions();
                }

                var outputText = ExtractOutputText(responseContent);

                Console.WriteLine("Parsed outputText:");
                Console.WriteLine(outputText);

                var suggestions = ParseSuggestions(outputText);

                if (!suggestions.Any())
                {
                    return GetDefaultSuggestions();
                }

                return suggestions;
            }
            catch (Exception ex)
            {
                Console.WriteLine("AiChatService hata:");
                Console.WriteLine(ex.Message);

                return GetDefaultSuggestions();
            }
        }

        private static string ExtractOutputText(string responseContent)
        {
            if (string.IsNullOrWhiteSpace(responseContent))
                return string.Empty;

            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            if (root.TryGetProperty("output_text", out var outputTextElement))
            {
                return outputTextElement.GetString() ?? string.Empty;
            }

            if (root.TryGetProperty("output", out var outputArray) &&
                outputArray.ValueKind == JsonValueKind.Array)
            {
                var sb = new StringBuilder();

                foreach (var item in outputArray.EnumerateArray())
                {
                    if (item.TryGetProperty("content", out var contentArray) &&
                        contentArray.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var content in contentArray.EnumerateArray())
                        {
                            if (content.TryGetProperty("text", out var textElement))
                            {
                                var text = textElement.GetString();
                                if (!string.IsNullOrWhiteSpace(text))
                                {
                                    sb.AppendLine(text.Trim());
                                }
                            }
                        }
                    }
                }

                return sb.ToString().Trim();
            }

            return string.Empty;
        }

        private static List<string> ParseSuggestions(string outputText)
        {
            if (string.IsNullOrWhiteSpace(outputText))
                return new List<string>();

            var suggestions = outputText
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Select(CleanSuggestionLine)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(3)
                .ToList();

            return suggestions;
        }

        private static string CleanSuggestionLine(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = text.Trim();

            while (text.StartsWith("-") || text.StartsWith("*"))
            {
                text = text.Substring(1).Trim();
            }

            if (text.Length > 2 && char.IsDigit(text[0]) &&
                (text[1] == '.' || text[1] == ')'))
            {
                text = text.Substring(2).Trim();
            }

            return text;
        }

        private static List<string> GetDefaultSuggestions()
        {
            return new List<string>
            {
                "Tamamdır 👍",
                "Kontrol edip döneyim",
                "Detay paylaşabilir misin?"
            };
        }
    }
}