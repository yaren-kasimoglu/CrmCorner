using System.Threading.Tasks;
using CrmCorner.Models.ChatCorner;

namespace CrmCorner.Services.ChatCorner
{
    public interface IAiSummaryService
    {
        Task<string> GenerateUserTaskSummaryCommentAsync(UserTaskSummaryDto summary);
    }
}