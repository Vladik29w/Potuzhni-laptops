using ErrorOr;
using LaptopServer.DB;
using LaptopServer.DTO;
using LaptopServer.Entities;
using LaptopServer.Mappers;
using Microsoft.EntityFrameworkCore;

namespace LaptopServer.Service
{
    public interface IOrderService
    {
        Task<ErrorOr<Guid>> CreateOrder(CreateOrderDTO creatingOrder, CancellationToken cancellationToken = default);
        Task<ErrorOr<OrderDTO>> GetOrder(Guid orderId);
        Task<List<OrderDTO>> GetAllOrders(); 
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

        public async Task<ErrorOr<Guid>> CreateOrder(CreateOrderDTO creatingOrder, CancellationToken cancellationToken = default)
        {
            if (creatingOrder.CartId == Guid.Empty)
                return Error.Validation(code: "NullCartID");

            var cart = await _cartService.GetCart(creatingOrder.CartId);
            if (cart == null)
                return Error.NotFound(code: "CartNotFound", description: $"Cart with ID '{creatingOrder.CartId}' not found.");
            if (cart.Items == null || !cart.Items.Any())
                return Error.Failure(code: "EmptyCart");
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
                PayMethod = creatingOrder.PayMethod,
                TotalPrice = cart.GrandTotal,
                DeliveryMethod = creatingOrder.DeliveryMethod,
                PhoneNumber = creatingOrder.PhoneNumber,
                ShippingAddress = creatingOrder.ShippingAddress,
                Email = creatingOrder.Email,
            };
            _dbContext.Add(order);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _cartService.ClearCart(creatingOrder.CartId);
            return order.Id;
        }
        public async Task<ErrorOr<OrderDTO>> GetOrder(Guid orderId)
        {

            var order = await _dbContext.Orders
                .AsNoTracking()
                .Where(ord => ord.Id == orderId)
                .ToOrder()
                .FirstOrDefaultAsync();

            if (order == null)
                return Error.NotFound(code: "OrderNotFound");
            return order;
        }
        public async Task<List<OrderDTO>> GetAllOrders()
        {
            return await _dbContext.Orders.AsNoTracking().ToOrder().ToListAsync();
        }
    }
}
