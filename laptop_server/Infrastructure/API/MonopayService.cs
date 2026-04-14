using ErrorOr;
using LaptopServer.DTO;
using LaptopServer.Enums;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LaptopServer.Infrastructure.API
{
    public interface IMonopayService
    {
        Task<ErrorOr<MonopayResponse>> CreateInvoice(OrderDTO order, CancellationToken ct = default);
        Task<ErrorOr<(Guid orderId, PaymentStatus status)>> CheckWebhook(string body, string xSign, CancellationToken ct = default);
    }
    public class MonopayService(HttpClient httpClient, IConfiguration configuration, IMemoryCache cache) : IMonopayService
    {
        private const string monoUrl = "https://api.monobank.ua/api/merchant";
        private const string webhook = "https://laptopserver-app-20260330234553.agreeableplant-e8507c58.polandcentral.azurecontainerapps.io/Webhook/getWebhook";
        private const string redirect = "https://potuzhni-laptops-atbmfyb4hafdhyb4.polandcentral-01.azurewebsites.net";

        public async Task<ErrorOr<MonopayResponse>> CreateInvoice(OrderDTO order, CancellationToken ct = default)
        {
            var req = new MonopayReq
            {
                RedirectUrl = $"{redirect}/{order.Id}",
                WebhookUrl = webhook,
                Amount = (int)(order.TotalPrice * 100),
                MerchantPaymInfo = new MerchantPaymInfo
                {
                    OrderId = order.Id.ToString(),
                    Destination = $"Payment for {order.Id}",
                    BasketOrder = order.OrderItems.Select(laptop => new BasketItem
                    {
                        Name = laptop.LaptopName,
                        Quantity = laptop.Quantity,
                        Sum = (int)(laptop.Price * 100),
                        LaptopId = laptop.LaptopId.ToString()
                    }
                    ).ToList()
                }
            };
            var response = await httpClient.PostAsJsonAsync($"{monoUrl}/invoice/create", req, ct);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<MonopayResponse>();
            }
            var errorJson = await response.Content.ReadAsStringAsync(ct);

            Console.WriteLine($"[MONO ERROR] Status: {response.StatusCode}, Body: {errorJson}");

            return Error.Failure(code: "Mono.ApiError", description: errorJson);
        }
        public async Task<ErrorOr<(Guid orderId, PaymentStatus status)>> CheckWebhook(string body, string xSign, CancellationToken ct = default)
        {
            if (!await VerifyResponse(body, xSign, ct))
                return Error.Failure(code: "MonoVerifyError");

            var data = JsonSerializer.Deserialize<MonopayWebhook>(body);
            if (data == null || !Guid.TryParse(data.Reference, out Guid orderId))
            {
                return Error.Validation(code: "NullGuid");
            }
            PaymentStatus status = data.Status switch
            {
                "pending" => PaymentStatus.Pending,
                "success" => PaymentStatus.Success,
                "failure" => PaymentStatus.Failure,
                "expired" => PaymentStatus.Expired,
                "reversed" => PaymentStatus.Reversed
            };
            return (orderId, status);
        }
        private async Task<bool> VerifyResponse(string body, string xSign, CancellationToken ct = default)
        {
            try
            {
                var chPubKey = await cache.GetOrCreateAsync("MonobankPubKey", async (entry) =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                    var response = await httpClient.GetFromJsonAsync<JsonNode>($"{monoUrl}/pubkey", cancellationToken: ct);
                    return response?["key"]?.ToString();
                });
                if (string.IsNullOrEmpty(chPubKey)) return false;
                byte[] keyBytes = Convert.FromBase64String(chPubKey);
                string keyPEM = Encoding.UTF8.GetString(keyBytes);

                using var ecdsa = ECDsa.Create();
                ecdsa.ImportFromPem(keyPEM);

                byte[] bodyBt = Encoding.UTF8.GetBytes(body);
                byte[] xSignBt = Convert.FromBase64String(xSign);

                return ecdsa.VerifyData(bodyBt, xSignBt, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence);
            }
            catch { return false; }

        }
    }
}
