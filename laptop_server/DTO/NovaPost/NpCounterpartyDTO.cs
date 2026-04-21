using System.Text.Json.Serialization;

namespace LaptopServer.DTO.NovaPost
{
    public record NpCounterpartyReq
    {
        [JsonPropertyName("FirstName")]
        public required string FirstName { get; set; }
        [JsonPropertyName("MiddleName")]
        public string MiddleName { get; set; } = string.Empty;
        [JsonPropertyName("LastName")]
        public required string LastName { get; set; }
        [JsonPropertyName("Phone")]
        public required string Phone { get; set; }
        [JsonPropertyName("Email")]
        public string? Email { get; set; }
        [JsonPropertyName("CounterpartyType")]
        public string CounterpartyType { get; set; } = "PrivatePerson";
        [JsonPropertyName("CounterpartyProperty")]
        public string CounterpartyProperty { get; set; } = "Recipient";
    }

    public record NpCounterpartyRes
    {
        [JsonIgnore]
        public Guid OrderId { get; set; }

        [JsonPropertyName("Ref")]
        public string Ref { get; set; } = string.Empty;

        [JsonPropertyName("Counterparty")]
        public string Counterparty { get; set; } = string.Empty;

        [JsonPropertyName("ContactPerson")]
        public NpRespone<NpCounterpartyContactPersonData> ContactPerson { get; set; } = new();
    }

    public record NpCounterpartyContactPersonData
    {
        [JsonPropertyName("Ref")]
        public string Ref { get; set; } = string.Empty;
    }
}
