using System.Threading.Tasks;
using CrmCorner.Models.ChatCorner;

namespace CrmCorner.Services.ChatCorner
{
    public interface IOpenAiIntentParserService
    {
        Task<ChatIntentParseResultDto> ParseIntentAsync(string question);
    }
}