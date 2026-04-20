using CrmCorner.Models;
using CrmCorner.Models.ChatCorner;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CrmCorner.Services.ChatCorner
{
    public class ChatCornerService : IChatCornerService
    {
        private readonly IChatAnalyticsService _chatAnalyticsService;
        private readonly IAiSummaryService _aiSummaryService;
        private readonly IChatAuthorizationService _chatAuthorizationService;
        private readonly CrmCornerContext _context;

        public ChatCornerService(
            IChatAnalyticsService chatAnalyticsService,
            IAiSummaryService aiSummaryService,
            IChatAuthorizationService chatAuthorizationService,
            CrmCornerContext context)
        {
            _chatAnalyticsService = chatAnalyticsService;
            _aiSummaryService = aiSummaryService;
            _chatAuthorizationService = chatAuthorizationService;
            _context = context;
        }

        public async Task<ChatCornerResponseDto> HandleQuestionAsync(string question, ClaimsPrincipal currentUser)
        {
            try
            {
                var currentUserId = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return new ChatCornerResponseDto
                    {
                        Success = false,
                        ErrorMessage = "Oturum bilgisi alınamadı."
                    };
                }

                var parsed = ParseQuestion(question);
                var lowerQuestion = (question ?? string.Empty).ToLowerInvariant();

                var isSelfQuery =
                    lowerQuestion.Contains("tasklarım") ||
                    lowerQuestion.Contains("durumum") ||
                    lowerQuestion.Contains("performansım") ||
                    lowerQuestion.Contains("benim task") ||
                    lowerQuestion.Contains("benim durumum") ||
                    lowerQuestion.Contains("bu ayki tasklarım") ||
                    lowerQuestion.Contains("geçen ay durumum") ||
                    lowerQuestion.Contains("bu yıl performansım");

                if (parsed == null && isSelfQuery)
                {
                    parsed = new ParsedChatQueryDto
                    {
                        Intent = "UserTaskSummary"
                    };

                    ResolveDateRange(lowerQuestion, parsed);
                }

                if (parsed == null)
                {
                    return new ChatCornerResponseDto
                    {
                        Success = false,
                        ErrorMessage = "Sizi daha iyi anlayabilmem için kullanıcı ve zaman bilgisini biraz daha net yazabilir misiniz? Örneğin: 'Bu ayki tasklarım ne durumda?', 'Yaren geçen ay nasıldı?' veya 'berat@saascorner.co 2025 yılında nasıl ilerledi?'"
                    };
                }

                if (parsed.Intent != "UserTaskSummary")
                {
                    return new ChatCornerResponseDto
                    {
                        Success = false,
                        ErrorMessage = "Bu analiz tipi henüz desteklenmiyor."
                    };
                }

                if (isSelfQuery && string.IsNullOrWhiteSpace(parsed.TargetEmail) && string.IsNullOrWhiteSpace(parsed.TargetName))
                {
                    var dbCurrentUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == currentUserId);
                    if (dbCurrentUser != null)
                    {
                        parsed.TargetEmail = dbCurrentUser.Email;
                        parsed.TargetName = dbCurrentUser.NameSurname;
                    }
                }

                var authResult = await _chatAuthorizationService.CanViewUserAsync(
    currentUserId,
    parsed.TargetEmail,
    parsed.TargetName);

                if (!authResult.Success)
                {
                    return new ChatCornerResponseDto
                    {
                        Success = false,
                        ErrorMessage = authResult.ErrorMessage
                    };
                }

                var summary = await _chatAnalyticsService.GetUserTaskSummaryAsync(
                    parsed.TargetEmail,
                    parsed.TargetName,
                    parsed.PeriodStart,
                    parsed.PeriodEnd);

                if (summary == null)
                {
                    return new ChatCornerResponseDto
                    {
                        Success = false,
                        ErrorMessage = "İlgili kullanıcı veya task verisi bulunamadı."
                    };
                }

                var aiAnswer = await _aiSummaryService.GenerateUserTaskSummaryCommentAsync(summary);

                return new ChatCornerResponseDto
                {
                    Success = true,
                    Intent = parsed.Intent,
                    Answer = aiAnswer,
                    Data = summary
                };
            }
            catch (Exception ex)
            {
                return new ChatCornerResponseDto
                {
                    Success = false,
                    ErrorMessage = "Bir hata oluştu: " + ex.Message
                };
            }
        }

        private ParsedChatQueryDto ParseQuestion(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return null;

            var text = question.Trim().ToLowerInvariant();

            var parsed = new ParsedChatQueryDto
            {
                Intent = "UserTaskSummary"
            };

            var emailMatch = Regex.Match(text, @"[a-z0-9._%+\-]+@[a-z0-9.\-]+\.[a-z]{2,}");
            if (emailMatch.Success)
            {
                parsed.TargetEmail = emailMatch.Value;
            }
            else
            {
                var namePatterns = new[]
                {
                    @"([a-zçğıöşü]+)'in",
                    @"([a-zçğıöşü]+)'ın",
                    @"([a-zçğıöşü]+)'un",
                    @"([a-zçğıöşü]+)'ün",
                    @"([a-zçğıöşü]+)\s+bu ay",
                    @"([a-zçğıöşü]+)\s+geçen ay",
                    @"([a-zçğıöşü]+)\s+bu yıl",
                    @"([a-zçğıöşü]+)\s+geçen yıl",
                    @"([a-zçğıöşü]+)\s+ocak",
                    @"([a-zçğıöşü]+)\s+şubat",
                    @"([a-zçğıöşü]+)\s+subat",
                    @"([a-zçğıöşü]+)\s+mart",
                    @"([a-zçğıöşü]+)\s+nisan",
                    @"([a-zçğıöşü]+)\s+mayıs",
                    @"([a-zçğıöşü]+)\s+mayis",
                    @"([a-zçğıöşü]+)\s+haziran",
                    @"([a-zçğıöşü]+)\s+temmuz",
                    @"([a-zçğıöşü]+)\s+ağustos",
                    @"([a-zçğıöşü]+)\s+agustos",
                    @"([a-zçğıöşü]+)\s+eylül",
                    @"([a-zçğıöşü]+)\s+eylul",
                    @"([a-zçğıöşü]+)\s+ekim",
                    @"([a-zçğıöşü]+)\s+kasım",
                    @"([a-zçğıöşü]+)\s+kasim",
                    @"([a-zçğıöşü]+)\s+aralık",
                    @"([a-zçğıöşü]+)\s+aralik"
                };

                foreach (var pattern in namePatterns)
                {
                    var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        parsed.TargetName = match.Groups[1].Value;
                        break;
                    }
                }
            }

            ResolveDateRange(text, parsed);

            if (string.IsNullOrWhiteSpace(parsed.TargetEmail) && string.IsNullOrWhiteSpace(parsed.TargetName))
                return null;

            return parsed;
        }

        private void ResolveDateRange(string text, ParsedChatQueryDto parsed)
        {
            var now = DateTime.Now;

            if (text.Contains("bu yıl"))
            {
                parsed.PeriodStart = new DateTime(now.Year, 1, 1);
                parsed.PeriodEnd = new DateTime(now.Year, 12, 31, 23, 59, 59);
                parsed.PeriodLabel = "Bu Yıl";
                return;
            }

            if (text.Contains("geçen yıl"))
            {
                var year = now.Year - 1;
                parsed.PeriodStart = new DateTime(year, 1, 1);
                parsed.PeriodEnd = new DateTime(year, 12, 31, 23, 59, 59);
                parsed.PeriodLabel = "Geçen Yıl";
                return;
            }

            if (text.Contains("bu ay"))
            {
                parsed.PeriodStart = new DateTime(now.Year, now.Month, 1);
                parsed.PeriodEnd = parsed.PeriodStart.AddMonths(1).AddSeconds(-1);
                parsed.PeriodLabel = "Bu Ay";
                return;
            }

            if (text.Contains("geçen ay"))
            {
                var d = now.AddMonths(-1);
                parsed.PeriodStart = new DateTime(d.Year, d.Month, 1);
                parsed.PeriodEnd = parsed.PeriodStart.AddMonths(1).AddSeconds(-1);
                parsed.PeriodLabel = "Geçen Ay";
                return;
            }

            var monthMap = new Dictionary<string, int>
            {
                { "ocak", 1 },
                { "şubat", 2 },
                { "subat", 2 },
                { "mart", 3 },
                { "nisan", 4 },
                { "mayıs", 5 },
                { "mayis", 5 },
                { "haziran", 6 },
                { "temmuz", 7 },
                { "ağustos", 8 },
                { "agustos", 8 },
                { "eylül", 9 },
                { "eylul", 9 },
                { "ekim", 10 },
                { "kasım", 11 },
                { "kasim", 11 },
                { "aralık", 12 },
                { "aralik", 12 }
            };

            foreach (var item in monthMap)
            {
                if (text.Contains(item.Key))
                {
                    var yearMatch = Regex.Match(text, @"20\d{2}");
                    var year = yearMatch.Success ? int.Parse(yearMatch.Value) : now.Year;

                    parsed.PeriodStart = new DateTime(year, item.Value, 1);
                    parsed.PeriodEnd = parsed.PeriodStart.AddMonths(1).AddSeconds(-1);
                    parsed.PeriodLabel = $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.Key)} {year}";
                    return;
                }
            }

            var onlyYearMatch = Regex.Match(text, @"\b(20\d{2})\b");
            if (onlyYearMatch.Success)
            {
                var year = int.Parse(onlyYearMatch.Value);
                parsed.PeriodStart = new DateTime(year, 1, 1);
                parsed.PeriodEnd = new DateTime(year, 12, 31, 23, 59, 59);
                parsed.PeriodLabel = year.ToString();
                return;
            }

            parsed.PeriodStart = new DateTime(now.Year, now.Month, 1);
            parsed.PeriodEnd = parsed.PeriodStart.AddMonths(1).AddSeconds(-1);
            parsed.PeriodLabel = "Bu Ay";
        }
    }
}