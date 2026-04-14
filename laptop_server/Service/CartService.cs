using ErrorOr;
using LaptopServer.DB;
using LaptopServer.DTO;
using LaptopServer.Entities;
using Microsoft.EntityFrameworkCore;

namespace LaptopServer.Service
{
    public interface ICartService
    {
        Task<CartDTO> GetCart(Guid cartId, CancellationToken ct = default);
        Task<ErrorOr<CartDTO>> AddToCart(Guid cartId, Guid laptopId, CancellationToken ct = default);
        Task<ErrorOr<CartDTO>> RemoveFromCart(Guid cartId, Guid laptopId, CancellationToken ct = default);
        Task<CartDTO> ClearCart(Guid cartId, CancellationToken ct = default);
    }
    public class CartService(LaptopsDBContext dbContext) : ICartService
    {
        public async Task<CartDTO> GetCart(Guid cartId, CancellationToken ct = default)
        {
            var cartItems = await dbContext.Carts
                .AsNoTracking()
                .Where(c => c.CartId == cartId)
                .Select(item => new CartItemDTO
                {
                    LaptopId = item.Laptop.Id,
                    LaptopName = item.Laptop.Name,
                    Price = item.Laptop.Price,
                    Quantity = item.Quantity,
                    TotalPrice = item.Laptop.Price * item.Quantity
                })
                .ToListAsync(ct);

            return new CartDTO
            {
                CartId = cartId,
                Items = cartItems,
                GrandTotal = cartItems.Sum(i => i.TotalPrice)
            };
        }
        public async Task<ErrorOr<CartDTO>> AddToCart(Guid cartId, Guid laptopId, CancellationToken ct = default)
        {
            if (await dbContext.Laptops.AnyAsync(i => i.Id == laptopId, ct))
            {
                var cartItem = await dbContext.Carts.FirstOrDefaultAsync(c => c.CartId == cartId && c.LaptopId == laptopId, ct);
                if (cartItem == null)
                {
                    cartItem = new CartItemEntity
                    {
                        CartId = cartId,
                        LaptopId = laptopId,
                        Quantity = 1
                    };
                    dbContext.Add(cartItem);
                }
                else
                    cartItem.Quantity++;
                await dbContext.SaveChangesAsync(ct);
                return await GetCart(cartId, ct);
            }
            else return Error.NotFound(code: "LaptopNotFound");
        }
        public async Task<ErrorOr<CartDTO>> RemoveFromCart(Guid cartId, Guid laptopId, CancellationToken ct = default)
        {
            var cartItem = await dbContext.Carts.FirstOrDefaultAsync(c => c.CartId == cartId && c.LaptopId == laptopId, ct);
            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                    cartItem.Quantity--;
                else
                    dbContext.Remove(cartItem);

                await dbContext.SaveChangesAsync(ct);
            }
            else return Error.NotFound(code: "CartItemNotFound");
            return await GetCart(cartId, ct);
        }
        public async Task<CartDTO> ClearCart(Guid cartId, CancellationToken ct = default)
        {
            await dbContext.Carts
           .Where(c => c.CartId == cartId)
           .ExecuteDeleteAsync(ct);
            return new CartDTO { CartId = cartId };
        }
    }
}
