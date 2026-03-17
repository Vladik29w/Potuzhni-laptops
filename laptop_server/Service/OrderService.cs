using LaptopServer.DB;
using LaptopServer.Entities;
using LaptopServer.Enums;

namespace LaptopServer.Service
{
    public interface IOrderService
    {
        Task<Guid> CreateOrder
            (Guid cartId, PayEnum pay, DeliveryEnum delivery, string phone, string email, string shippingAddress, CancellationToken cancellationToken = default);
    }

    public class OrderService : IOrderService
    {
        private readonly ICartService _cartService;

        private readonly LaptopsDBContext _dbContext;
        public OrderService(LaptopsDBContext dbContext, ICartService сartService)
        {
            _dbContext = dbContext;
            _cartService = сartService;
        }

        public async Task<Guid> CreateOrder
            (Guid cartId, PayEnum pay, DeliveryEnum delivery, string phone, string email, string shippingAddress, CancellationToken cancellationToken = default)
        {
            if (cartId == Guid.Empty)
                throw new ArgumentException("Cart ID cannot be empty.", nameof(cartId));

            var cart = await _cartService.GetCart(cartId);
            if (cart == null)
                throw new InvalidOperationException($"Cart with ID '{cartId}' not found.");
            if (cart.Items == null || !cart.Items.Any())
                throw new InvalidOperationException("Cannot create an order from an empty cart.");
            var orderItems = cart.Items.Select(item => new OrderItemEntity
            {
                LaptopId = item.LaptopId,
                LaptopName = item.LaptopName,
                Price = item.Price,
                Quantity = item.Quantity,
            }).ToList();
            var order = new OrderEntity
            {
                Id = Guid.NewGuid(),
                OrderItems = orderItems,
                PayMethod = pay,
                TotalPrice = cart.GrandTotal,
                DeliveryMethod = delivery,
                PhoneNumber = phone,
                ShippingAddress = shippingAddress,
                Email = email,
            };
            _dbContext.Add(order);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _cartService.ClearCart(cartId);
            return order.Id;
        }
    }
}
