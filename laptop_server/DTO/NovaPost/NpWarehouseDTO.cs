using System.Text.Json.Serialization;

namespace LaptopServer.DTO.NovaPost
{
    public record NpGetWarehouseReq
    {
        [JsonPropertyName("SettlementRef")]
        public required string SettlementRef { get; set; }
        public string? CityRef { get; set; }

        [JsonPropertyName("FindByString")]
        public string? FindByString { get; set; }

        [JsonPropertyName("Page")]
        public string Page { get; set; } = "1";

        [JsonPropertyName("Limit")]
        public string Limit { get; set; } = "100";
    }
    public record NpWarehouse
    {
        [JsonPropertyName("Description")]
        public string? Description { get; set; }
        [JsonPropertyName("Ref")]
        public required string Ref { get; set; }
        [JsonPropertyName("CityRef")]
        public required string CityRef { get; set; }
        [JsonPropertyName("TypeOfWarehouseRef")]
        public string? TypeOfWarehouseRef { get; set; }
    }
}
