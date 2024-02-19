using CrmCorner.Models;

namespace CrmCorner
{
    public class NotificationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

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

                var salesToNotify = dbContext.TaskComps
                    .Where(s => s.SalesDone.HasValue && s.SalesDone.Value.Date == DateTime.Today.AddDays(3))
                    .ToList();

                //foreach (var sale in salesToNotify)
                //{
                //    var notification = new Notification
                //    {
                //        UserId = sale.UserId, // Burada TaskComp modelinizde bir UserId özelliği olduğunu varsayıyorum.
                //        Message = $"Satış kapatma tarihinize 3 gün kaldı: {sale.Title}",
                //        // Diğer alanlar...
                //    };

                //    dbContext.Notifications.Add(notification);
                //}

                await dbContext.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
