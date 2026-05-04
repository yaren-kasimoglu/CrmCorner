namespace CrmCorner.Models.Settings
{
    public class OpenAISettings
    {
        public string ApiKey { get; set; }

        public string Model { get; set; } = "gpt-4.1-mini";

        public string BaseUrl { get; set; } = "https://api.openai.com/v1/";
    }
}