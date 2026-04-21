using CrmCorner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrmCorner.Services
{
    public class TodoDeadlineReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public TodoDeadlineReminderService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<CrmCornerContext>();
                        var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

                        var now = DateTime.Now;

                        var tasks = await context.TodoEntries
                            .Include(x => x.AssigneeUser)
                            .Include(x => x.AssignedByUser)
                            .Include(x => x.TodoBoard)
                            .Where(x => !x.IsDone && !x.IsDayBoardTask && x.Deadline.HasValue)
                            .ToListAsync(stoppingToken);

                        foreach (var task in tasks)
                        {
                            var targetUser = task.AssigneeUser;

                            if (targetUser == null)
                            {
                                targetUser = await context.Users
                                    .FirstOrDefaultAsync(u => u.Id == task.UserId, stoppingToken);
                            }

                            if (targetUser == null || string.IsNullOrWhiteSpace(targetUser.Email))
                                continue;

                            var remaining = task.Deadline.Value - now;

                            if (remaining.TotalMinutes <= 0)
                                continue;

                            // 1 hafta kala
                            if (!task.DeadlineReminderWeekSent &&
                                remaining.TotalDays <= 7 &&
                                remaining.TotalDays > 3)
                            {
                                await emailService.SendTodoDeadlineReminderEmailAsync(
                                    targetUser.Email,
                                    targetUser.NameSurname,
                                    task.Text,
                                    "1 hafta kaldı",
                                    task.Deadline.Value);

                                task.DeadlineReminderWeekSent = true;
                            }

                            // 3 gün kala
                            if (!task.DeadlineReminder3DaysSent &&
                                remaining.TotalDays <= 3 &&
                                remaining.TotalDays > 1)
                            {
                                await emailService.SendTodoDeadlineReminderEmailAsync(
                                    targetUser.Email,
                                    targetUser.NameSurname,
                                    task.Text,
                                    "3 gün kaldı",
                                    task.Deadline.Value);

                                task.DeadlineReminder3DaysSent = true;
                            }

                            // son gün
                            if (!task.DeadlineReminderLastDaySent &&
                                remaining.TotalDays <= 1 &&
                                remaining.TotalHours > 2)
                            {
                                await emailService.SendTodoDeadlineReminderEmailAsync(
                                    targetUser.Email,
                                    targetUser.NameSurname,
                                    task.Text,
                                    "Son gün",
                                    task.Deadline.Value);

                                task.DeadlineReminderLastDaySent = true;
                            }

                            // 2 saat kala
                            if (!task.DeadlineReminder2HoursSent &&
                                remaining.TotalHours <= 2 &&
                                remaining.TotalMinutes > 0)
                            {
                                await emailService.SendTodoDeadlineReminderEmailAsync(
                                    targetUser.Email,
                                    targetUser.NameSurname,
                                    task.Text,
                                    "2 saat kaldı",
                                    task.Deadline.Value);

                                task.DeadlineReminder2HoursSent = true;
                            }
                        }

                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
                catch
                {
                }

                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }
    }
}