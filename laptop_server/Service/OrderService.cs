using ErrorOr;
using LaptopServer.DB;
using LaptopServer.DTO;
using LaptopServer.Enums;
using LaptopServer.Infrastructure.API;
using Microsoft.EntityFrameworkCore;

namespace LaptopServer.Service
{
    public interface IOrderService
    {
        Task<ErrorOr<OrderDTO>> CreateOrder(CreateOrderDTO creatingOrder, CancellationToken cancellationToken = default);
        Task UpdateOrder(Guid orderId, PaymentStatus status);
        Task<ErrorOr<OrderDTO>> GetOrder(Guid orderId);
        Task<List<OrderDTO>> GetAllOrders();
    }
    public class OrderService : IOrderService
    {
        private readonly ICartService _cartService;
        private readonly IMonopayService _payService;

        private readonly LaptopsDBContext _dbContext;
        public OrderService(LaptopsDBContext dbContext, ICartService сartService, IMonopayService monopay)
        {
            _dbContext = dbContext;
            _cartService = сartService;
            _payService = monopay;
        }
        public async Task<ErrorOr<OrderDTO>> CreateOrder(CreateOrderDTO creatingOrder, CancellationToken cancellationToken = default)
        {
            if (creatingOrder.CartId == Guid.Empty)
                return Error.Validation(code: "NullCartID");

            var cart = await _cartService.GetCart(creatingOrder.CartId);
            if (cart == null)
                return Error.NotFound(code: "CartNotFound", description: $"Cart with ID '{creatingOrder.CartId}' not found.");
            if (cart.Items == null || !cart.Items.Any())
                return Error.Failure(code: "EmptyCart");

            Guid orderId = Guid.NewGuid();
            var order = OrderMapper.ToOrderEntity(creatingOrder, cart, orderId);
            var orderdto = OrderMapper.ToDto(order);
            var payRes = await _payService.CreateInvoice(orderdto);
            if (payRes.IsError)
                return payRes.Errors;
            order.PaymentId = payRes.Value.InvoiceId;
            order.PaymentUrl = payRes.Value.PageUrl;
            await _dbContext.SaveChangesAsync(); //todo перепиши по юніт оф ворк
            await _cartService.ClearCart(creatingOrder.CartId);
            return OrderMapper.ToDto(order);
        }
        public async Task UpdateOrder(Guid orderId, PaymentStatus status)
        {
            await _dbContext.Orders.Where(ord => ord.Id == orderId).ExecuteUpdateAsync(set => set.SetProperty(ord => ord.PaymentStatus, status));
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
