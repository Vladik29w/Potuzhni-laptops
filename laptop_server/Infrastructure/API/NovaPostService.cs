using ErrorOr;
using LaptopServer.DTO.NovaPost;
using Microsoft.Extensions.Caching.Memory;

namespace LaptopServer.Infrastructure.API
{
    public interface INovaPostService
    {
        Task<ErrorOr<List<NpSettlementAddress>>> GetCities(string cityName, CancellationToken ct = default);
        Task<ErrorOr<List<NpWarehouse>>> GetWarehouses(string settlementRef, string? searchString = null, CancellationToken ct = default);
    }
    public class NovaPostService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration) : INovaPostService
    {
        private const string ApiUrl = "https://api.novaposhta.ua/v2.0/json/";
        private readonly string _apiKey = configuration["ApiKeys:NovaPost"] ?? throw new InvalidOperationException("Null API key");

        public async Task<ErrorOr<List<NpSettlementAddress>>> GetCities(string cityName, CancellationToken ct = default)
        {
            string cacheKey = $"np_city_{cityName.ToLowerInvariant()}";
            if (cache.TryGetValue(cacheKey, out List<NpSettlementAddress>? chCities))
                return chCities!;
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
                var data = res.Data.First().Addresses;
                cache.Set(cacheKey, data, TimeSpan.FromDays(3));
                return data;
            }
            return Error.Failure(code: "GetCityFail");
        }
        public async Task<ErrorOr<List<NpWarehouse>>> GetWarehouses(string settlementRef, string? searchString = null, CancellationToken ct = default)
        {
            string chSearch = searchString ?? "all";
            string cacheKey = $"np_warehouse_{settlementRef}_{chSearch.ToLowerInvariant()}";
            if (cache.TryGetValue(cacheKey, out List<NpWarehouse>? chCities))
                return chCities!;
            var req = new NpReq<NpGetWarehouseReq>
            {
                ApiKey = _apiKey,
                ModelName = "Address",
                CalledMethod = "getWarehouses",
                MethodProperties = new NpGetWarehouseReq
                {
                    SettlementRef = settlementRef,
                    FindByString = searchString,
                }
            };
            var response = await httpClient.PostAsJsonAsync(ApiUrl, req, ct);
            response.EnsureSuccessStatusCode();

            var res = await response.Content.ReadFromJsonAsync<NpRespone<NpWarehouse>>();

            if (res != null && res.Success)
            {
                var data = res.Data;
                cache.Set(cacheKey, data, TimeSpan.FromHours(6));
                return data;
            }
            return Error.Failure(code: "GetWarehouseFail");
        }
    }
}
