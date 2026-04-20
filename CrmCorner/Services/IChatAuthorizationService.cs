using System.Threading.Tasks;

namespace CrmCorner.Services.ChatCorner
{
    public interface IChatAuthorizationService
    {
        Task<(bool Success, string ErrorMessage)> CanViewUserAsync(string currentUserId, string targetEmail, string targetName);
    }
}