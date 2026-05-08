using Microsoft.Extensions.DependencyInjection;

namespace CrmCorner.Services.DailyReporting
{
    public class WeeklyDailyReportBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WeeklyDailyReportBackgroundService> _logger;

        public WeeklyDailyReportBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<WeeklyDailyReportBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Weekly Daily Report Background Service başladı.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var nextRun = GetNextMondayAtEight(DateTime.Now);
                    var delay = nextRun - DateTime.Now;

                    _logger.LogInformation("Weekly report sonraki çalışma zamanı: {NextRun}", nextRun);

                    await Task.Delay(delay, stoppingToken);

                    if (stoppingToken.IsCancellationRequested)
                        break;

                    using var scope = _serviceProvider.CreateScope();

                    var reportService = scope.ServiceProvider
                        .GetRequiredService<WeeklyDailyReportEmailService>();

                    await reportService.SendLastWeekReportsAsync();

                    _logger.LogInformation("Weekly report mail işlemi tamamlandı.");
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Weekly report mail gönderilirken hata oluştu.");

                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        private DateTime GetNextMondayAtEight(DateTime now)
        {
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;

            var nextMonday = now.Date.AddDays(daysUntilMonday).AddHours(8);

            if (nextMonday <= now)
                nextMonday = nextMonday.AddDays(7);

            return nextMonday;
        }
    }
}