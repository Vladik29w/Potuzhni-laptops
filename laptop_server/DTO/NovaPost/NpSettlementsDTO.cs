using System.Text.Json.Serialization;

namespace LaptopServer.DTO.NovaPost
{
    public record NpSettlementsReq
    {
        [JsonPropertyName("CityName")]
        public required string CityName { get; set; }
        [JsonPropertyName("Limit")]
        public string Limit { get; set; } = "50";
        [JsonPropertyName("Page")]
        public string Page { get; set; } = "1";
    }
    public record NpSettlementsData
    {
        [JsonPropertyName("TotalCount")]
        public int TotalCount { get; set; } = 0;
        [JsonPropertyName("Addresses")]
        public List<NpSettlementAddress> Addresses { get; set; } = new();
    }
    public record NpSettlementAddress
    {
        [JsonPropertyName("Present")]
        public required string Present { get; set; }
        [JsonPropertyName("DeliveryCity")]
        public required string DeliveryCity { get; set; }
        [JsonPropertyName("Ref")]
        public required string Ref { get; set; }
        [JsonPropertyName("MainDescription")]
        public required string MainDescription { get; set; }
    }

}
