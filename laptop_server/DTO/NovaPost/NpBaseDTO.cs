using System.Text.Json.Serialization;

namespace LaptopServer.DTO.NovaPost
{
    public record NpReq<T>
    {
        [JsonPropertyName("apiKey")]
        public required string ApiKey { get; init; }
        [JsonPropertyName("modelName")]
        public required string ModelName { get; init; }
        [JsonPropertyName("calledMethod")]
        public required string CalledMethod { get; init; }
        [JsonPropertyName("methodProperties")]
        public required T MethodProperties { get; init; }
    }
    public record NpRespone<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }
        [JsonPropertyName("data")]
        public List<T> Data { get; init; } = new List<T>();
        [JsonPropertyName("errors")]
        public List<string> Errors { get; init; } = new List<string>();
    }
}
