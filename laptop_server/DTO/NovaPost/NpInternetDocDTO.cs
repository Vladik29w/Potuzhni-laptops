using System.Text.Json.Serialization;

namespace LaptopServer.DTO.NovaPost
{
    public record NpInternetDocReq
    {
        [JsonPropertyName("PayerType")]
        public string PayerType { get; set; } = "Sender";

        [JsonPropertyName("PaymentMethod")]
        public string PaymentMethod { get; set; } = "Cash";

        [JsonPropertyName("DateTime")]
        public string DateTime { get; init; } = System.DateTime.Now.Hour >= 17
            ? System.DateTime.Now.AddDays(1).ToString("dd.MM.yyyy")
            : System.DateTime.Now.ToString("dd.MM.yyyy");

        [JsonPropertyName("CargoType")]
        public string CargoType { get; set; } = "Parcel";

        [JsonPropertyName("VolumeGeneral")]
        public required string VolumeGeneral { get; set; } // це із orderItem

        [JsonPropertyName("Weight")]
        public required string Weight { get; set; } // це із orderItem

        [JsonPropertyName("ServiceType")]
        public string ServiceType { get; set; } = "WarehouseWarehouse";

        [JsonPropertyName("SeatsAmount")]
        public string SeatsAmount { get; set; } = "1";

        [JsonPropertyName("Description")]
        public string Description { get; set; } = "Електричні прилади";

        [JsonPropertyName("Cost")]
        public required string Cost { get; set; } // це GrandTotal із orderEntity

        [JsonPropertyName("Sender")]
        public string Sender { get; set; } = "49250516-2e81-11f1-a1d5-48df37b921da";

        [JsonPropertyName("ContactSender")]
        public string ContactSender { get; set; } = "4925d363-2e81-11f1-a1d5-48df37b921da";

        [JsonPropertyName("SendersPhone")]
        public string SendersPhone { get; set; } = "380997384342";

        [JsonPropertyName("CitySender")]
        public string CitySender { get; set; } = "8d5a980d-391c-11dd-90d9-001a92567626";

        [JsonPropertyName("SenderAddress")]
        public string SenderAddress { get; set; } = "47402e89-e1c2-11e3-8c4a-0050568002cf";

        [JsonPropertyName("Recipient")]
        public required string Recipient { get; set; }

        [JsonPropertyName("ContactRecipient")]
        public required string ContactRecipient { get; set; }

        [JsonPropertyName("RecipientsPhone")]
        public required string RecipientsPhone { get; set; }

        [JsonPropertyName("CityRecipient")]
        public required string CityRecipient { get; set; }

        [JsonPropertyName("RecipientAddress")]
        public required string RecipientAddress { get; set; }
    }

    public record NpContactPersonData
    {
        [JsonPropertyName("Ref")]
        public string Ref { get; set; } = string.Empty;
        [JsonPropertyName("Description")]
        public string Description { get; set; } = string.Empty;
    }

    public record NpInternetDocRes
    {
        [JsonPropertyName("Ref")] 
        public string Ref { get; set; } = string.Empty;
        [JsonPropertyName("IntDocNumber")] 
        public string IntDocNumber { get; set; } = string.Empty;
    }
}