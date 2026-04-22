namespace LaptopServer.DTO
{
    public record LaptopMainDTO
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public decimal Price { get; init; } = 0;
        public required string Img { get; init; }
    }

    public record LaptopDetailsDTO
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public decimal Price { get; init; } = 0;
        public string Img { get; init; } = "maybe img placeholder?";
        public string CPU { get; init; } = "cpu";
        public int RAM { get; init; } = 0;
        public string GPU { get; init; } = "gpu";
        public string? DiskSize { get; init; }
        public double? ScreenSize { get; init; }
        public string? ScreenResolution { get; init; }
        public int? ScreenRefresh { get; init; }
        public int? Battery { get; init; }
    }
    public record LaptopAdminDTO
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public required decimal Price { get; set; }
        public required string Img { get; set; }
        public required string CPU { get; set; }
        public required int RAM { get; set; } 
        public required string GPU { get; set; }
        public string? DiskSize { get; set; }
        public double? ScreenSize { get; set; }
        public string? ScreenResolution { get; set; }
        public int? ScreenRefresh { get; set; }
        public int? Battery { get; set; }
    }
}