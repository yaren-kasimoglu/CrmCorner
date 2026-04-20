using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CrmCorner.Models;
using CrmCorner.Models.ChatCorner;
using CrmCorner.Models.Enums;

namespace CrmCorner.Services.ChatCorner
{
    public class ChatAnalyticsService : IChatAnalyticsService
    {
        private readonly CrmCornerContext _context;

        public ChatAnalyticsService(CrmCornerContext context)
        {
            _context = context;
        }

        public async Task<UserTaskSummaryDto> GetUserTaskSummaryAsync(string email, string name, DateTime startDate, DateTime endDate)
        {
            AppUser user = null;

            if (!string.IsNullOrWhiteSpace(email))
            {
                user = await _context.Users
                    .FirstOrDefaultAsync(x => x.Email != null && x.Email.ToLower() == email.ToLower());
            }
            else if (!string.IsNullOrWhiteSpace(name))
            {
                user = await _context.Users
                    .FirstOrDefaultAsync(x => x.NameSurname != null && x.NameSurname.ToLower().Contains(name.ToLower()));
            }

            if (user == null)
                return null;

            var tasks = await _context.PipelineTasks
                .Where(x =>
                    (x.AppUserId == user.Id || x.ResponsibleUserId == user.Id) &&
                    x.CreatedDate >= startDate &&
                    x.CreatedDate <= endDate)
                .ToListAsync();

            var dto = new UserTaskSummaryDto
            {
                UserId = user.Id,
                UserEmail = user.Email,
                UserName = user.NameSurname,
                PeriodStart = startDate,
                PeriodEnd = endDate,
                TotalTasks = tasks.Count,

                DegerlendirilenCount = tasks.Count(x => x.Stage == PipelineStage.Degerlendirilen),
                GorusmeBaslatildiCount = tasks.Count(x => x.Stage == PipelineStage.IletisimKuruldu),
                ToplantiDuzenlendiCount = tasks.Count(x => x.Stage == PipelineStage.ToplantiDuzenlendi),
                TeklifSunulduCount = tasks.Count(x => x.Stage == PipelineStage.TeklifSunuldu),
                SonucAsamasiCount = tasks.Count(x => x.Stage == PipelineStage.Sonuc),

                WonCount = tasks.Count(x => x.OutcomeStatus == OutcomeTypeSales.Won),
                LostCount = tasks.Count(x => x.OutcomeStatus == OutcomeTypeSales.Lost)
            };

            var decidedCount = dto.WonCount + dto.LostCount;
            dto.WinRate = decidedCount > 0
                ? Math.Round((decimal)dto.WonCount * 100 / decidedCount, 2)
                : (decimal?)null;

            return dto;
        }
    }
}