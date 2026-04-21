using Azure;
using ErrorOr;
using LaptopServer.DTO.NovaPost;
using LaptopServer.Service;
using LaptopServer.Services.Background;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Threading.Channels;

namespace LaptopServer.Infrastructure.API.NovaPost
{
    public interface INovaPostApiService
    {
        Task<ErrorOr<List<NpSettlementAddress>>> GetCities(string cityName, CancellationToken ct = default);
        Task<ErrorOr<NpCounterpartyRes>> CreateCounterparty(Guid orderId, CancellationToken ct = default);
        Task WriteChannelWarehouses(ChannelWriter<NpWarehouse> channelWriter, CancellationToken ct = default);
        Task<ErrorOr<NpInternetDocRes>> CreateInternetDoc(NpInternetDocReq internetDocReq, CancellationToken ct = default);
    }

    public class NovaPostApiService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration, IOrderService orderService, ILogger<NovaPostApiService> logger) : INovaPostApiService
    {
        private const string ApiUrl = "https://api.novaposhta.ua/v2.0/json/";
        private readonly string _apiKey = configuration["ApiKeys:NovaPost"] ?? throw new InvalidOperationException("Null API key");

        private async Task<NpRespone<T>?> SendNovaPostRequest<T>(object request, CancellationToken ct = default)
        {
            var response = await httpClient.PostAsJsonAsync(ApiUrl, request, ct);
            if (!response.IsSuccessStatusCode) return null;
            var jsonString = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<NpRespone<T>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<ErrorOr<List<NpSettlementAddress>>> GetCities(string cityName, CancellationToken ct = default)
        {
            string cacheKey = $"np_city_{cityName.ToLowerInvariant()}";

            var result = await cache.GetOrCreateAsync(cacheKey, async entry =>
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

                var res = await SendNovaPostRequest<NpSettlementsData>(req, ct);

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
        private static string FormatPhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

            if (digits.Length == 9)
                digits = $"380{digits}";
            else if (digits.Length == 10 && digits.StartsWith("0"))
                digits = $"38{digits}";
            else if (digits.Length > 12)
            {
                var countryCodeIndex = digits.IndexOf("380", StringComparison.Ordinal);
                if (countryCodeIndex >= 0 && digits.Length - countryCodeIndex >= 12)
                    digits = digits.Substring(countryCodeIndex, 12);
            }

            return digits;
        }

        public async Task<ErrorOr<NpCounterpartyRes>> CreateCounterparty(Guid orderId, CancellationToken ct = default)
        {
            var customerRes = await orderService.GetCustomerInfo(orderId, ct);

            if (customerRes.IsError)
                return customerRes.Errors;

            var customer = customerRes.Value;
            var formattedPhone = FormatPhoneNumber(customer.PhoneNumber);

            if (formattedPhone.Length != 12 || !formattedPhone.StartsWith("380", StringComparison.Ordinal))
                return Error.Validation(code: "InvalidPhoneNumber", description: $"Invalid phone number format for order '{orderId}'.");

            var req = new NpReq<NpCounterpartyReq>
            {
                ApiKey = _apiKey,
                ModelName = "Counterparty",
                CalledMethod = "save",
                MethodProperties = new NpCounterpartyReq
                {
                    FirstName = customer.FirstName,
                    MiddleName = customer.MiddleName,
                    LastName = customer.LastName,
                    Phone = formattedPhone,
                    Email = customer.Email  
                }
            };

            var res = await SendNovaPostRequest<NpCounterpartyRes>(req, ct);

            if (res == null)
                return Error.Failure(code: "CreateCounterpartyFail", description: "Nova Post returned empty response when creating counterparty.");

            if (!res.Success)
            {
                var errors = res.Errors.Any() ? string.Join("; ", res.Errors) : "Unknown API error";
                logger.LogError("Nova Post counterparty creation failed for order {OrderId}: {Errors}", orderId, errors);
                return Error.Failure(code: "CreateCounterpartyFail", description: errors);
            }

            if (res.Data != null && res.Data.Count > 0)
            {
                var counterparty = res.Data.First();
                counterparty.OrderId = orderId;//???
                return counterparty;
            }

            return Error.Failure(code: "CreateCounterpartyFail", description: "Counterparty data was not returned by Nova Post.");
        }
        public async Task<ErrorOr<NpInternetDocRes>> CreateInternetDoc(NpInternetDocReq internetDocReq, CancellationToken ct = default)
        {
            var req = new NpReq<NpInternetDocReq>
            {
                ApiKey = _apiKey,
                ModelName = "InternetDocument",
                CalledMethod = "save",
                MethodProperties = internetDocReq
            };
            var res = await SendNovaPostRequest<NpInternetDocRes>(req, ct);
            if (res != null && res.Success && res.Data != null && res.Data.Count > 0)
            {
                return res.Data.First();
            }
            return Error.Failure(code: "CreateInternetDocFail");
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

                    var res = await SendNovaPostRequest<NpWarehouse>(req, ct);

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
    }
}
