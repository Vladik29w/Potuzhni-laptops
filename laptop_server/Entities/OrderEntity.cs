using LaptopServer.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LaptopServer.Entities
{
    public class OrderEntity
    {
        public Guid Id { get; set; }
        public virtual ICollection<OrderItemEntity> OrderItems { get; set; } = new List<OrderItemEntity>();
        [Range(typeof(decimal), "0", "99999999.99")]
        [Precision(18, 2)]
        public decimal TotalPrice { get; set; } = 0;
        public PayEnum PayMethod { get; set; } = PayEnum.Unknown;
        public DeliveryEnum DeliveryMethod { get; set; } = DeliveryEnum.Unknown;
        [Phone]
        [MaxLength(16)]
        public required string PhoneNumber { get; set; }
        [EmailAddress]
        [MaxLength(64)]
        public string? Email { get; set; }
        [MaxLength(36)]
        public string? DeliveryCityRef { get; set; }
        [MaxLength(128)]
        public string? DeliveryCityName { get; set; }
        [MaxLength(36)]
        public string? DeliveryWarehouseRef { get; set; }
        [MaxLength(255)]
        public string? DeliveryWarehouseName { get; set; }
        public string? PaymentId { get; set; }
        public string? PaymentUrl { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    public class OrderItemEntity
    {
        public int Id { get; init; }
        public Guid LaptopId { get; init; }
        [MaxLength(128)]
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
