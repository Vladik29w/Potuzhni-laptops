using ErrorOr;
using LaptopServer.DTO.NovaPost;

namespace LaptopServer.Infrastructure.API
{
    public interface INovaPostService
    {
        Task<ErrorOr<List<NpSettlementAddress>>> GetCities(string cityName);
        Task<ErrorOr<List<NpWarehouse>>> GetWarehouses(string settlementRef, string? searchString = null);
    }
    public class NovaPostService : INovaPostService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string ApiUrl = "https://api.novaposhta.ua/v2.0/json/";
        public NovaPostService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["ApiKeys:NovaPost"] ?? throw new InvalidOperationException("Null API key");
        }
        public async Task<ErrorOr<List<NpSettlementAddress>>> GetCities(string cityName)
        {
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
            var response = await _httpClient.PostAsJsonAsync(ApiUrl, req);
            response.EnsureSuccessStatusCode();

            var res = await response.Content.ReadFromJsonAsync<NpRespone<NpSettlementsData>>();

            if (res != null && res.Success && res.Data.Count > 0)
                return res.Data.First().Addresses;

            return Error.Failure(code: "GetCityFail");
        }
        public async Task<ErrorOr<List<NpWarehouse>>> GetWarehouses(string settlementRef, string? searchString = null)
        {
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
            var response = await _httpClient.PostAsJsonAsync(ApiUrl, req);
            response.EnsureSuccessStatusCode();

            var res = await response.Content.ReadFromJsonAsync<NpRespone<NpWarehouse>>();

            if (res != null && res.Success)
                return res.Data;

            return Error.Failure(code: "GetWarehouseFail");


        }
    }
}
