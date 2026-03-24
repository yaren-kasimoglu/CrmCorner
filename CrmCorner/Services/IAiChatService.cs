using CrmCorner.Models;

namespace CrmCorner.Services
{
    public interface IAiChatService
    {
        Task<List<string>> GenerateReplySuggestionsAsync(List<ChatHistory> messages, string currentUserId);
    }
}