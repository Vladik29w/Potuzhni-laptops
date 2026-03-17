namespace LaptopServer.DTO
{
    public record LaptopMainDTO
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public decimal Price { get; init; } = 0;
        public required string Img { get; init; }
    }

    public record LaptopDetailsDTO
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public decimal Price { get; init; } = 0;
        public string Img { get; init; } = "maybe img placeholder?";
        public string CPU { get; init; } = "cpu";
        public int RAM { get; init; } = 0;
        public string GPU { get; init; } = "gpu";
    }
}