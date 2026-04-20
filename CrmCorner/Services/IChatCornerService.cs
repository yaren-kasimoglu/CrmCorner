using System.Security.Claims;
using System.Threading.Tasks;
using CrmCorner.Models.ChatCorner;

namespace CrmCorner.Services.ChatCorner
{
    public interface IChatCornerService
    {
        Task<ChatCornerResponseDto> HandleQuestionAsync(string question, ClaimsPrincipal currentUser);
    }
}