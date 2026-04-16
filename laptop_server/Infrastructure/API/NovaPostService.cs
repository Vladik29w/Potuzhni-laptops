using EFCore.BulkExtensions;
using ErrorOr;
using LaptopServer.DB;
using LaptopServer.DTO.NovaPost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Channels;

namespace LaptopServer.Infrastructure.API
{
    public interface INovaPostService
    {
        Task<ErrorOr<List<NpSettlementAddress>>> GetCities(string cityName, CancellationToken ct = default);
        Task<ErrorOr<List<NpWarehouse>>> GetWarehouses(string settlementRef, string? searchString = null, CancellationToken ct = default);
        Task WriteChannelWarehouses(ChannelWriter<NpWarehouse> channelWriter, CancellationToken ct = default);
        Task ReadChannelWarehouse(ChannelReader<NpWarehouse> channelReader, CancellationToken ct = default);
    }
    public class NovaPostService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration, IServiceProvider serviceProvider, LaptopsDBContext dbContext) : INovaPostService
    {
        private const string ApiUrl = "https://api.novaposhta.ua/v2.0/json/";
        private readonly string _apiKey = configuration["ApiKeys:NovaPost"] ?? throw new InvalidOperationException("Null API key");

        public async Task<ErrorOr<List<NpSettlementAddress>>> GetCities(string cityName, CancellationToken ct = default)
        {
            string cacheKey = $"np_city_{cityName.ToLowerInvariant()}";

            var result = await cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromDays(3);

                var req = new NpReq<NpSettlementsReq>
                {
                    ApiKey = _apiKey,
                    ModelName = "Address",
                    CalledMethod = "searchSettlements",
                    MethodProperties = new NpSettlementsReq
                    {
                        CityName = cityName
                    }
                };
                var response = await httpClient.PostAsJsonAsync(ApiUrl, req, ct);
                response.EnsureSuccessStatusCode();

                var res = await response.Content.ReadFromJsonAsync<NpRespone<NpSettlementsData>>();

                if (res != null && res.Success && res.Data.Count > 0)
                {
                    return res.Data.First().Addresses;
                }
                return null;
            });

            if (result != null)
                return result;

            return Error.Failure(code: "GetCityFail");
        }
        public async Task<ErrorOr<List<NpWarehouse>>> GetWarehouses(string settlementRef, string? searchString = null, CancellationToken ct = default)
        {
            string chSearch = searchString ?? "all";
            string cacheKey = $"np_warehouse_{settlementRef}_{chSearch}";

            var result = await cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(6);

                try
                {
                    var query = dbContext.NpWarehouses.Where(w => w.SettlementRef == settlementRef);

                    if (!string.IsNullOrWhiteSpace(searchString) && searchString != "all")
                    {
                        query = query.Where(w =>
                            w.Ref.Contains(searchString) ||
                            (w.Description != null && w.Description.Contains(searchString)));
                    }

                    var warehouses = await query.ToListAsync(ct);

                    var data = warehouses.Select(e => new NpWarehouse
                    {
                        Ref = e.Ref,
                        SettlementRef = e.SettlementRef,
                        Description = e.Description,
                        TypeOfWarehouseRef = e.TypeOfWarehouseRef
                    }).ToList();

                    return data;
                }
                catch
                {
                    return null;
                }
            });

            if (result != null)
                return result;

            return Error.Failure(code: "GetWarehouseFail");
        }
        public async Task WriteChannelWarehouses(ChannelWriter<NpWarehouse> channelWriter, CancellationToken ct = default)
        {
            int page = 1;
            const string limit = "5000";
            bool more = true;
            try
            {
                while (more == true && !ct.IsCancellationRequested)
                {
                    var req = new NpReq<NpGetWarehouseReq>
                    {
                        ApiKey = _apiKey,
                        ModelName = "Address",
                        CalledMethod = "getWarehouses",
                        MethodProperties = new NpGetWarehouseReq
                        {
                            Page = page.ToString(),
                            Limit = limit,
                        }
                    };
                    page++;
                    var response = await httpClient.PostAsJsonAsync(ApiUrl, req, ct);
                    response.EnsureSuccessStatusCode();

                    var res = await response.Content.ReadFromJsonAsync<NpRespone<NpWarehouse>>(cancellationToken: ct);

                    if (res == null || !res.Success || res.Data == null || res.Data.Count == 0)
                    {
                        more = false;
                    }
                    else
                    {
                        foreach (var warehouse in res.Data)
                        {
                            await channelWriter.WriteAsync(warehouse, ct);
                        }
                    }
                }
            }
            finally { channelWriter.TryComplete(); }
        }
        public async Task ReadChannelWarehouse(ChannelReader<NpWarehouse> channelReader, CancellationToken ct = default)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<LaptopsDBContext>();
            const int batchSize = 5000;
            var buffer = new List<Entities.NpWarehousesEntity>(batchSize);
            var bulkConfig = new BulkConfig
            {
                BatchSize = batchSize,
                SetOutputIdentity = false,
                UpdateByProperties = [nameof(Entities.NpWarehousesEntity.Ref)],
                CalculateStats = false
            };

            await foreach (var data in channelReader.ReadAllAsync(ct))
            {
                buffer.Add(Mappers.NovaPostMapper.ToWarehouseEntity(data));

                if (buffer.Count >= batchSize)
                {
                    await dbContext.BulkInsertOrUpdateAsync(buffer, bulkConfig: bulkConfig, cancellationToken: ct);
                    buffer.Clear();
                }
            }

            if (buffer.Count > 0)
            {
                await dbContext.BulkInsertOrUpdateAsync(buffer, bulkConfig: bulkConfig, cancellationToken: ct);
            }
        }
    }
}
