using LaptopServer.DTO.NovaPost;
using LaptopServer.Infrastructure.API.NovaPost;
using System.Threading.Channels;

namespace LaptopServer.Services.Background_services
{
    public class NpCacheBgService(IServiceScopeFactory scopeFactory, ILogger<NpCacheBgService> logger, IHostApplicationLifetime lifetime) : BackgroundService
    {
        private readonly PeriodicTimer timer = new(TimeSpan.FromDays(1));

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Yield();
                await Task.Delay(Timeout.Infinite, lifetime.ApplicationStarted);
            }
            catch (OperationCanceledException) { }

            logger.LogInformation("Np cache updater started");

            try
            {
                await WaitUntilNextNight(stoppingToken);
                await UpdateNpWarehouses(stoppingToken);

                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await UpdateNpWarehouses(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Np cache updater stopped");
            }
        }

        private static async Task WaitUntilNextNight(CancellationToken ct)
        {
            var now = DateTime.Now;
            var nextRun = now.Date.AddDays(1).AddHours(2);
            var delay = nextRun - now;

            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, ct);
        }

        private async Task UpdateNpWarehouses(CancellationToken ct)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                var novaPostApi = scope.ServiceProvider.GetRequiredService<INovaPostApiService>();
                var novaPostDb = scope.ServiceProvider.GetRequiredService<INovaPostDbService>();

                var channel = Channel.CreateBounded<NpWarehouse>(new BoundedChannelOptions(5000)
                {
                    SingleReader = true,
                    SingleWriter = true,
                    FullMode = BoundedChannelFullMode.Wait,
                });

                Task writerTask = novaPostApi.WriteChannelWarehouses(channel.Writer, ct);
                Task readerTask = novaPostDb.ReadChannelWarehouse(channel.Reader, ct);

                await Task.WhenAll(writerTask, readerTask);

                logger.LogInformation("Np warehouses cache updated");
            }
            catch (OperationCanceledException) { }
            catch
            {
                logger.LogWarning("Np warehouses cache update failed");
            }
            sw.Stop();
            logger.LogInformation($"Total time to sync: {sw.Elapsed.TotalSeconds.ToString("F2")}");
        }

        public override void Dispose()
        {
            timer.Dispose();
            base.Dispose();
        }
    }
}
