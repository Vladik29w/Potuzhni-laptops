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
        public required int Amount { get; set; } //price kopeyki
        [JsonPropertyName("merchantPaymInfo")]
        public required MerchantPaymInfo MerchantPaymInfo { get; set; }
        [JsonPropertyName("redirectUrl")]
        public required string RedirectUrl { get; set; }
        [JsonPropertyName("webhookUrl")]
        public required string WebhookUrl { get; set; }
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
        public required string OrderId {  get; set; }
        [JsonPropertyName("destination")]
        public string Destination {  get; set; }
        [JsonPropertyName("basketOrder")]
        public List<BasketItem> BasketOrder { get; set; } = new();
    }
    public record BasketItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("qty")]
        public int Quantity { get; set; } = 1;
        [JsonPropertyName("sum")]
        public int Sum { get; set; } = 0;
        [JsonPropertyName("code")]
        public string LaptopId { get; set; } = string.Empty;
    }

}
