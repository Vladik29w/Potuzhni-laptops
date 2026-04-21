using LaptopServer.Infrastructure.API.NovaPost;
using LaptopServer.Infrastructure.Notification;
using ErrorOr;

namespace LaptopServer.Services.Background
{
    public class NpInternetDocBgService(IServiceScopeFactory scopeFactory, ILogger<NpInternetDocBgService> logger, OrderProcessingChannel channel) : BackgroundService
    {
        private static string FormatErrors(List<Error> errors)
            => string.Join("; ", errors.Select(err => $"{err.Code}: {err.Description}"));

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var orderId in channel.Reader.ReadAllAsync(stoppingToken))
            {
                logger.LogInformation($"{orderId} has been confirmed");
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var npApi = scope.ServiceProvider.GetRequiredService<INovaPostApiService>();
                    var npDb = scope.ServiceProvider.GetRequiredService<INovaPostDbService>();
                    var counterparty = await npApi.CreateCounterparty(orderId, stoppingToken);

                    if (counterparty.IsError)
                    {
                        logger.LogError("Failed to create counterparty for order {OrderId}: {Errors}", orderId, FormatErrors(counterparty.Errors));
                        continue;
                    }

                    logger.LogInformation("Counterparty created");
                    await npDb.SaveCounterparty(counterparty.Value, stoppingToken);
                    logger.LogInformation(counterparty.Value.Counterparty);
                    logger.LogInformation(counterparty.Value.Ref);
                    logger.LogInformation("Counterparty saved");

                    var internetDocReq = await npDb.FillInternetDoc(orderId, stoppingToken);
                    if (internetDocReq.IsError)
                    {
                        logger.LogError("Failed to fill internet document for order {OrderId}: {Errors}", orderId, FormatErrors(internetDocReq.Errors));
                        continue;
                    }

                    var internetDoc = await npApi.CreateInternetDoc(internetDocReq.Value, stoppingToken);
                    if (internetDoc.IsError)
                    {
                        logger.LogError("Failed to create internet document for order {OrderId}: {Errors}", orderId, FormatErrors(internetDoc.Errors));
                        continue;
                    }

                    logger.LogInformation("Internted document created");
                    await npDb.SaveInternetDoc(internetDoc.Value, orderId, stoppingToken);
                    logger.LogInformation("Internted document saved");
                }
                catch(Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    logger.LogError(ex, "Failed to create internet document");
                }
            }
        }
    }
}
