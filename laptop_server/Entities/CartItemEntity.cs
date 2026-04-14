using System.ComponentModel.DataAnnotations;

namespace LaptopServer.Entities
{
    public class CartEntity
    {
        public Guid Id { get; set; }
        public List<CartItemEntity> CartItems { get; set; } = new List<CartItemEntity>();
        public DateTime Updated { get; private set; } = DateTime.UtcNow;
        public void Refresh() => Updated = DateTime.UtcNow;
    }
    public class CartItemEntity
    {
        public Guid CartId { get; set; }
        public virtual CartEntity Cart { get; set; } = null!;
        public Guid LaptopId { get; set; }
        public virtual LaptopEntity Laptop { get; init; } = null!;
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }
}
