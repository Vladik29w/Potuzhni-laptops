using LaptopServer.Enums;
using System.ComponentModel.DataAnnotations;

namespace LaptopServer.DTO
{
    public record OrderDTO
    {
        public Guid Id { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; } = new List<OrderItemDTO>();
        public required decimal TotalPrice { get; set; }
        public PayEnum PayMethod { get; set; } = PayEnum.Unknown;
        public DeliveryEnum DeliveryMethod { get; set; } = DeliveryEnum.Unknown;
        public required CustomerInfoDTO CustomerInfo { get; set;  }
        public string? DeliveryCityRef { get; set; }
        public string? DeliveryCityName { get; set; }
        public string? DeliveryWarehouseRef { get; set; }
        public string? DeliveryWarehouseName { get; set; }
        public string? PaymentId { get; set; }
        public string? PaymentUrl { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    public record OrderItemDTO
    {
        public int Id { get; init; }
        public Guid LaptopId { get; init; }
        public required string LaptopName { get; init; }
        public required decimal Price { get; init; }
        public int Quantity { get; init; } = 1;
        public Guid OrderId { get; init; }
    }
    public record CreateOrderDTO
    {
        public Guid CartId { get; init; }
        public PayEnum PayMethod { get; init; }
        public DeliveryEnum DeliveryMethod { get; init; }
        public required CustomerInfoDTO CustomerInfo { get; init; }
        public string? DeliveryCityRef { get; init; }
        public string? DeliveryCityName { get; init; }
        public string? DeliveryWarehouseRef { get; init; }
        public string? DeliveryWarehouseName { get; init; }
    }
    public record CustomerInfoDTO
    {
        [MaxLength(32)]
        public required string FirstName { get; set; }
        [MaxLength(32)]
        public string MiddleName { get; set; } = string.Empty;
        [MaxLength(32)]
        public required string LastName { get; set; }
        [Phone]
        [MaxLength(16)]
        public required string PhoneNumber { get; set; }
        [EmailAddress]
        [MaxLength(64)]
        public string? Email { get; set; }
    }
    public record OrderStatsDTO(DateTime Date, int Quantity, decimal Sum);
}