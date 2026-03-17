namespace LaptopServer.DTO
{
    public record CartItemDTO
    {
        public required string LaptopId { get; init; }
        public required string LaptopName { get; init; }
        public decimal Price { get; init; } = 0;
        public int Quantity { get; init; } = 1;
        public decimal TotalPrice { get; init; } = 0;
    }
    public record CartDTO
    {
        public Guid CartId { get; init; }
        public List<CartItemDTO> Items { get; init; } = new();
        public decimal GrandTotal { get; init; } = 0;
    }
}
