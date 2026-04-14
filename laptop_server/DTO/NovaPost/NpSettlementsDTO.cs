using System.Text.Json.Serialization;

namespace LaptopServer.DTO.NovaPost
{
    public record NpSettlementsReq
    {
        [JsonPropertyName("CityName")]
        public required string CityName { get; init; }
        [JsonPropertyName("Limit")]
        public string Limit { get; init; } = "50";
        [JsonPropertyName("Page")]
        public string Page { get; init; } = "1";
    }
    public record NpSettlementsData
    {
        [JsonPropertyName("TotalCount")]
        public int TotalCount { get; init; } = 0;
        [JsonPropertyName("Addresses")]
        public List<NpSettlementAddress> Addresses { get; init; } = new();
    }
    public record NpSettlementAddress
    {
        [JsonPropertyName("Present")]
        public required string Present { get; init; }
        [JsonPropertyName("DeliveryCity")]
        public required string DeliveryCity { get; init; }
        [JsonPropertyName("Ref")]
        public required string Ref { get; init; }
        [JsonPropertyName("MainDescription")]
        public required string MainDescription { get; init; }
    }

}
