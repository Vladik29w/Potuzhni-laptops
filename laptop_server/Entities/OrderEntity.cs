using LaptopServer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public bool IsConfirmed { get; set; } = false;
        [ComplexType]
        public record CustomerInfoType
        {
            [MaxLength(32)]
            public required string FirstName { get; set; }
            [MaxLength(32)]
            public string MiddleName { get; set; } = string.Empty;
            [MaxLength(32)]
            public required string LastName { get; set;  }
            [Phone]
            [MaxLength(16)]
            public required string PhoneNumber { get; set; }
            [EmailAddress]
            [MaxLength(64)]
            public string? Email { get; set; }
        }
        public required CustomerInfoType CustomerInfo { get; set; }
        public string? DeliveryCityRef { get; set; }
        [MaxLength(128)]
        public string? DeliveryCityName { get; set; }
        [MaxLength(36)]
        public string? DeliveryWarehouseRef { get; set; }
        [MaxLength(255)]
        public string? DeliveryWarehouseName { get; set; }
        public string? NpRecipientRef { get; set; }
        public string? NpContactRecipientRef { get; set; }
        public string? TrackingDocRef { get; set; }
        public string? TrackingDocNum { get; set; }
        [MaxLength(20)]
        public string? PaymentId { get; set; }
        public string? PaymentUrl { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
    }
    public class OrderItemEntity
    {
        public int Id { get; set; }
        public Guid LaptopId { get; init; }
        [MaxLength(64)]
        public required string LaptopName { get; init; }
        [Precision(18, 2)]
        [Range(typeof(decimal), "0", "9999999.99")]
        public required decimal Price { get; init; }
        public double Weight { get; init; }
        public double VolumeGeneral { get; init; }
        [Range(0, int.MaxValue)]
        public int Quantity { get; init; } = 1;
        public Guid OrderId { get; set; }
        public virtual OrderEntity Order { get; init; } = null!;
    }
}
