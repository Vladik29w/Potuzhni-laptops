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
        Task UpdateOrder(Guid orderId, PaymentStatus status, CancellationToken ct = default);
        Task<ErrorOr<OrderDTO>> GetOrder(Guid orderId, CancellationToken ct = default);
        Task<IReadOnlyList<OrderDTO>> GetAllOrders(CancellationToken ct = default);
    }
    public class OrderService(LaptopsDBContext dbContext, ICartService cartService, IMonopayService monopay) : IOrderService
    {
        public async Task<ErrorOr<OrderDTO>> CreateOrder(CreateOrderDTO creatingOrder, CancellationToken ct = default)
        {
            if (creatingOrder.CartId == Guid.Empty)
                return Error.Validation(code: "NullCartID");

            var cart = await cartService.GetCart(creatingOrder.CartId, ct);
            if (cart == null)
                return Error.NotFound(code: "CartNotFound", description: $"Cart with ID '{creatingOrder.CartId}' not found.");
            if (cart.Items == null || !cart.Items.Any())
                return Error.Failure(code: "EmptyCart");

            await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                Guid orderId = Guid.NewGuid();
                var order = OrderMapper.ToOrderEntity(creatingOrder, cart, orderId);
                order.PaymentStatus = PaymentStatus.Pending;

                await dbContext.AddAsync(order, ct);
                await dbContext.SaveChangesAsync(ct);

                var orderdto = OrderMapper.ToDto(order);
                var payRes = await monopay.CreateInvoice(orderdto, ct);

                if (payRes.IsError)
                {
                    await transaction.RollbackAsync(ct);
                    return payRes.Errors;
                }

                order.PaymentId = payRes.Value.InvoiceId;
                order.PaymentUrl = payRes.Value.PageUrl;
                await dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                await cartService.ClearCart(creatingOrder.CartId, ct);
                return OrderMapper.ToDto(order);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(ct);
                return Error.Failure(code: "OrderCreatingErr");
            }
        }
        public async Task UpdateOrder(Guid orderId, PaymentStatus status, CancellationToken ct = default)
        {
            await dbContext.Orders
            .Where(ord => ord.Id == orderId && ord.PaymentStatus != PaymentStatus.Success && ord.PaymentStatus != PaymentStatus.Reversed)
            .ExecuteUpdateAsync(set => set.SetProperty(ord => ord.PaymentStatus, status), ct);
        }
        public async Task<ErrorOr<OrderDTO>> GetOrder(Guid orderId, CancellationToken ct = default)
        {

            var order = await dbContext.Orders
                .AsNoTracking()
                .Where(ord => ord.Id == orderId)
                .ToOrder()
                .FirstOrDefaultAsync(ct);

            if (order == null)
                return Error.NotFound(code: "OrderNotFound");
            return order;
        }
        public async Task<IReadOnlyList<OrderDTO>> GetAllOrders(CancellationToken ct = default)
        {
            return await dbContext.Orders.AsNoTracking().ToOrder().ToListAsync(ct);
        }
    }
}
