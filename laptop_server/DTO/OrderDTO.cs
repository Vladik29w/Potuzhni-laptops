using LaptopServer.Entities;
using LaptopServer.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace LaptopServer.DTO
{
    public record OrderDTO
    {
        public Guid Id { get; init; }
        public List<OrderItemDTO> OrderItems { get; set; } = new List<OrderItemDTO>();
        public required decimal TotalPrice { get; set; }
        public PayEnum PayMethod { get; set; } = PayEnum.Unknown;
        public DeliveryEnum DeliveryMethod { get; set; } = DeliveryEnum.Unknown;
        public required string PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? ShippingAddress { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    public record OrderItemDTO
    {
        public int Id { get; init; }
        public required string LaptopId { get; init; }
        public required string LaptopName { get; init; }
        public required decimal Price { get; init; }
        public int Quantity { get; init; } = 1;
        public Guid OrderId { get; init; }
        public virtual OrderEntity Order { get; init; } = null!;
    }
    public record CreateOrderDTO
    {
        public Guid CartId { get; init; }
        public PayEnum PayMethod { get; init; }
        public DeliveryEnum DeliveryMethod { get; init; }
        public required string PhoneNumber { get; init; }
        public string? Email { get; set; }
        public string? ShippingAddress { get; set; }
    }
    public record OrderStatsDTO(DateTime Date, int Quantity, decimal Sum);
}
