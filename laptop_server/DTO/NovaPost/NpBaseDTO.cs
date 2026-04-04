using System.Text.Json.Serialization;

namespace LaptopServer.DTO.NovaPost
{
    public record NpReq<T>
    {
        [JsonPropertyName("apiKey")]
        public required string ApiKey { get; set; }
        [JsonPropertyName("modelName")]
        public required string ModelName { get; set; }
        [JsonPropertyName("calledMethod")]
        public required string CalledMethod { get; set; }
        [JsonPropertyName("methodProperties")]
        public required T MethodProperties { get; set; }
    }
    public record NpRespone<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("data")]
        public List<T> Data { get; set; } = new List<T>();
        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; } = new List<string>();
    }
}
