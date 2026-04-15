using LaptopServer.DB;
using Microsoft.EntityFrameworkCore;

namespace LaptopServer.Services.Background_services
{
    public class CartCleanerService(IServiceScopeFactory scopeFactory, ILogger<CartCleanerService> logger, IHostApplicationLifetime lifetime) : BackgroundService
    {
        private readonly PeriodicTimer timer = new(TimeSpan.FromSeconds(10));
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Yield();
                await Task.Delay(Timeout.Infinite, lifetime.ApplicationStarted);
            }
            catch (OperationCanceledException) { }
            logger.LogInformation("Cart cleaner start");

            try
            {
                await CleanCart(stoppingToken);
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await CleanCart(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Cart cleaner shutted down");
            }
        }
        private async Task CleanCart(CancellationToken ct)
        {
            try
            {
                using (IServiceScope scope = scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<LaptopsDBContext>();
                    var threshold = DateTime.UtcNow.AddDays(-3);

                    int delCount = await dbContext.Carts
                        .Where(c => c.Updated < threshold)
                        .ExecuteDeleteAsync(ct);

                    if (delCount > 0)
                        logger.LogInformation($"Carts deleted: {delCount}");
                }
            }
            catch (OperationCanceledException) { }
            catch
            {
                logger.LogWarning("Error while clearing cart");
            }
        }
        public override void Dispose()
        {
            timer.Dispose();
            base.Dispose();
        }
    }
}
