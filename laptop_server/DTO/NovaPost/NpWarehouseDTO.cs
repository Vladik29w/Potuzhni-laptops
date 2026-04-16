using System.Text.Json.Serialization;

namespace LaptopServer.DTO.NovaPost
{
    public record NpGetWarehouseReq
    {
        [JsonPropertyName("SettlementRef")]
        public string? SettlementRef { get; init; }
        public string? CityRef { get; init; }

        [JsonPropertyName("FindByString")]
        public string? FindByString { get; init; }

        [JsonPropertyName("Page")]
        public string Page { get; init; } = "1";

        [JsonPropertyName("Limit")]
        public string Limit { get; init; } = "100";
    }
    public record NpWarehouse
    {
        [JsonPropertyName("Description")]
        public string? Description { get; init; }
        [JsonPropertyName("Ref")]
        public required string Ref { get; init; }
        [JsonPropertyName("SettlementRef")]
        public required string SettlementRef { get; init; }
        [JsonPropertyName("TypeOfWarehouseRef")]
        public string? TypeOfWarehouseRef { get; init; }
    }
}
