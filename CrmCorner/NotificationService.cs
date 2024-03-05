using CrmCorner.Models;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner
{
    public class NotificationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _systemUserId = "b0ce3c93-e4e8-495f-b1e6-5ef5c54a8d63";


        public NotificationService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await DoWork(stoppingToken);
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken); // Her gün kontrol et
            }
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<CrmCornerContext>();

                try
                {
                    var todayPlusThreeDays = DateTime.Today.AddDays(3);
                    var salesToNotify = dbContext.TaskComps
                        .Include(tc => tc.AppUser) // Eğer TaskComp ve AppUser arasında ilişki varsa
                        .Where(tc => tc.SalesDone.HasValue && tc.SalesDone.Value.Date == todayPlusThreeDays)
                        .ToList();

                    foreach (var sale in salesToNotify)
                    {

                        // Önce, bu satış için zaten bir bildirim olup olmadığını kontrol edin
                        var alreadyNotified = dbContext.Notifications.Any(n => n.TaskCompId == sale.TaskId); 

                        if (!alreadyNotified)
                        {
                            var notification = new Notification
                            {
                                AppUserId = sale.AppUser?.Id ?? _systemUserId,
                                Message = $"Satış kapatma tarihinize 3 gün kaldı: {sale.Title}",
                                DateCreated = DateTime.Now,
                                IsRead = false
                                // Diğer alanlar...
                            };

                            if (notification.AppUserId == null)
                            {
                                throw new InvalidOperationException("AppUserId cannot be null for a notification.");
                            }

                            dbContext.Notifications.Add(notification);
                        }
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    // Hata loglamayı burada gerçekleştirin
                    // Örneğin: LogError(ex.Message);
                    // Hata yönetimi uygulamanızın loglama yapısına bağlıdır, bu nedenle uygun loglama mekanizmanızı buraya entegre edin.
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }




    }
}
