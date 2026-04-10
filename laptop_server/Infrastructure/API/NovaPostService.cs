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
    public class NovaPostService : INovaPostService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache; //todo cache getOrCreate
        private readonly string _apiKey;
        private const string ApiUrl = "https://api.novaposhta.ua/v2.0/json/";
        public NovaPostService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _cache = cache;
            _apiKey = configuration["ApiKeys:NovaPost"] ?? throw new InvalidOperationException("Null API key");
        }
        public async Task<ErrorOr<List<NpSettlementAddress>>> GetCities(string cityName, CancellationToken ct = default)
        {
            string cacheKey = $"np_city_{cityName.ToLowerInvariant()}";
            if (_cache.TryGetValue(cacheKey, out List<NpSettlementAddress>? chCities))
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
            var response = await _httpClient.PostAsJsonAsync(ApiUrl, req, ct);
            response.EnsureSuccessStatusCode();

            var res = await response.Content.ReadFromJsonAsync<NpRespone<NpSettlementsData>>();

            if (res != null && res.Success && res.Data.Count > 0)
            {
                var data = res.Data.First().Addresses;
                _cache.Set(cacheKey, data, TimeSpan.FromDays(3));
                return data;
            }
            
            return Error.Failure(code: "GetCityFail");
        }
        public async Task<ErrorOr<List<NpWarehouse>>> GetWarehouses(string settlementRef, string? searchString = null, CancellationToken ct = default)
        {
            string chSearch = searchString ?? "all"; 
            string cacheKey = $"np_warehouse_{settlementRef}_{chSearch.ToLowerInvariant()}";
            if (_cache.TryGetValue(cacheKey, out List<NpWarehouse>? chCities))
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
            var response = await _httpClient.PostAsJsonAsync(ApiUrl, req, ct);
            response.EnsureSuccessStatusCode();

            var res = await response.Content.ReadFromJsonAsync<NpRespone<NpWarehouse>>();

            if (res != null && res.Success)
            {
                var data = res.Data;
                _cache.Set(cacheKey, data, TimeSpan.FromHours(6));
                return data;
            }

            return Error.Failure(code: "GetWarehouseFail");


        }
    }
}
