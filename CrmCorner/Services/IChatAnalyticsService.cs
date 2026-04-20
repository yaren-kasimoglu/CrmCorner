using System;
using System.Threading.Tasks;
using CrmCorner.Models.ChatCorner;

namespace CrmCorner.Services.ChatCorner
{
    public interface IChatAnalyticsService
    {
        Task<UserTaskSummaryDto> GetUserTaskSummaryAsync(
            string email,
            string name,
            DateTime startDate,
            DateTime endDate);
    }
}