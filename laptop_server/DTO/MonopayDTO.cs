using LaptopServer.Enums;
using System.Text.Json.Serialization;

namespace LaptopServer.DTO
{
    public record MonopayResponse
    {
        [JsonPropertyName("invoiceId")]
        public string InvoiceId { get; set; } = string.Empty;
        [JsonPropertyName("pageUrl")]
        public string PageUrl { get; set; } = string.Empty;
    }
    public record MonopayReq
    {
        [JsonPropertyName("amount")]
        public required int Amount { get; init; }
        [JsonPropertyName("merchantPaymInfo")]
        public required MerchantPaymInfo MerchantPaymInfo { get; init; }
        [JsonPropertyName("redirectUrl")]
        public required string RedirectUrl { get; init; }
        [JsonPropertyName("webhookUrl")]
        public required string WebhookUrl { get; init; }
    }
    public record MonopayWebhook
    {
        [JsonPropertyName("invoiceId")]
        public string InvoiceId { get; init; } = string.Empty;
        [JsonPropertyName("status")]
        public string Status { get; init; } = string.Empty;
        [JsonPropertyName("reference")]
        public string Reference { get; init; } = string.Empty;
        [JsonPropertyName("amount")]
        public int Amount { get; init; }
    }
    public record MerchantPaymInfo
    {
        [JsonPropertyName("reference")]
        public required string OrderId { get; init; }
        [JsonPropertyName("destination")]
        public string Destination { get; init; } = string.Empty;
        [JsonPropertyName("basketOrder")]
        public List<BasketItem> BasketOrder { get; init; } = new();
    }
    public record BasketItem
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;
        [JsonPropertyName("qty")]
        public int Quantity { get; init; } = 1;
        [JsonPropertyName("sum")]
        public int Sum { get; init; } = 0;
        [JsonPropertyName("code")]
        public string LaptopId { get; init; } = string.Empty;
    }

}
