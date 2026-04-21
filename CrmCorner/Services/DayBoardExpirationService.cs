using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CrmCorner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CrmCorner.Services
{
    public class DayBoardExpirationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DayBoardExpirationService(IServiceScopeFactory scopeFactory)
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

                        // TEST için: silinmeye 1 dakika kala uyarı gönderilecek görevler
                        var warningTasks = await context.TodoEntries
                            .Where(x =>
                                x.IsDayBoardTask &&
                                x.ExpiresAt.HasValue &&
                                !x.ExpirationWarningSent &&
                                x.ExpiresAt.Value > now &&
                                x.ExpiresAt.Value <= now.AddMinutes(1))
                            .ToListAsync(stoppingToken);

                        foreach (var task in warningTasks)
                        {
                            var user = await context.Users
                                .FirstOrDefaultAsync(u => u.Id == task.UserId, stoppingToken);

                            if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                            {
                                await emailService.SendTodoExpirationWarningAsync(
                                    user.Email,
                                    task.Text,
                                    task.ExpiresAt
                                );
                            }

                            task.ExpirationWarningSent = true;
                        }

                        // Süresi dolmuş görevleri sil
                        var expiredTasks = await context.TodoEntries
                            .Where(x =>
                                x.IsDayBoardTask &&
                                x.ExpiresAt.HasValue &&
                                x.ExpiresAt.Value <= now)
                            .ToListAsync(stoppingToken);

                        if (expiredTasks.Any())
                        {
                            context.TodoEntries.RemoveRange(expiredTasks);
                        }

                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Servis hata verdi: " + ex.Message);
                }

                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
                // Canlıda bunu sonra 1 dakikaya çıkaracağız
                // await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}