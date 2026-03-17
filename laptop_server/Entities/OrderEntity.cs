using LaptopServer.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LaptopServer.Entities
{
    public class OrderEntity
    {
        public Guid Id { get; init; }
        public virtual ICollection<OrderItemEntity> OrderItems { get; init; } = new List<OrderItemEntity>();
        [Range(typeof(decimal), "0", "99999999.99")]
        [Precision(18, 2)]
        public required decimal TotalPrice { get; set; }
        public PayEnum PayMethod { get; set; } = PayEnum.Unknown;
        public DeliveryEnum DeliveryMethod { get; set; } = DeliveryEnum.Unknown;
        [Phone]
        [MaxLength(16)]
        public required string PhoneNumber { get; set; }
        [EmailAddress]
        [MaxLength(256)]
        public string? Email { get; set; }
        [MaxLength(512)]
        public string? ShippingAddress { get; set; }
    }
    public class OrderItemEntity
    {
        public int Id { get; init; }
        [MaxLength(256)]
        public required string LaptopId { get; init; }
        [MaxLength(512)]
        public required string LaptopName { get; init; }
        [Precision(18, 2)]
        [Range(typeof(decimal), "0", "9999999.99")]
        public required decimal Price { get; init; }
        [Range(0, int.MaxValue)]
        public int Quantity { get; init; } = 1;
        public Guid OrderId { get; init; }
        public virtual OrderEntity Order { get; init; } = null!;
    }
}
