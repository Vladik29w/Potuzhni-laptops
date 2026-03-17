using System.ComponentModel.DataAnnotations;

namespace LaptopServer.Entities
{
    public class CartItemEntity
    {
        public Guid CartId { get; set; }
        [MaxLength(256)]
        public required string LaptopId { get; set; }
        public virtual LaptopEntity Laptop { get; init; } = null!;
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }
}
