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
            var cart = await dbContext.Carts
                .AsNoTracking()
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Laptop)
                .FirstOrDefaultAsync(c => c.Id == cartId, ct);

            if (cart == null)
            {
                return new CartDTO
                {
                    CartId = cartId,
                    Items = [],
                    GrandTotal = 0
                };
            }

            var items = cart.CartItems.Select(item => new CartItemDTO
            {
                LaptopId = item.Laptop.Id,
                LaptopName = item.Laptop.Name,
                Price = item.Laptop.Price,
                Quantity = item.Quantity,
                TotalPrice = item.Laptop.Price * item.Quantity
            }).ToList();

            return new CartDTO
            {
                CartId = cartId,
                Items = items,
                GrandTotal = items.Sum(i => i.TotalPrice)
            };
        }

        public async Task<ErrorOr<CartDTO>> AddToCart(Guid cartId, Guid laptopId, CancellationToken ct = default)
        {
            if (!await dbContext.Laptops.AnyAsync(i => i.Id == laptopId, ct))
                return Error.NotFound(code: "LaptopNotFound");

            var cart = await dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cartId, ct);

            if (cart == null)
            {
                cart = new CartEntity { Id = cartId };
                dbContext.Carts.Add(cart);
            }

            var cartItem = cart.CartItems.FirstOrDefault(i => i.LaptopId == laptopId);
            if (cartItem == null)
            {
                cart.CartItems.Add(new CartItemEntity
                {
                    CartId = cartId,
                    LaptopId = laptopId,
                    Quantity = 1
                });
            }
            else
            {
                cartItem.Quantity++;
            }

            cart.Refresh(); 
            await dbContext.SaveChangesAsync(ct);

            return await GetCart(cartId, ct);
        }

        public async Task<ErrorOr<CartDTO>> RemoveFromCart(Guid cartId, Guid laptopId, CancellationToken ct = default)
        {
            var cart = await dbContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cartId, ct);

            if (cart == null)
                return Error.NotFound(code: "CartNotFound");

            var cartItem = cart.CartItems.FirstOrDefault(i => i.LaptopId == laptopId);
            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                    cartItem.Quantity--;
                else
                    cart.CartItems.Remove(cartItem);

                cart.Refresh(); 
                await dbContext.SaveChangesAsync(ct);
            }
            else
            {
                return Error.NotFound(code: "CartItemNotFound");
            }

            return await GetCart(cartId, ct);
        }

        public async Task<CartDTO> ClearCart(Guid cartId, CancellationToken ct = default)
        {
            await dbContext.Carts
                .Where(c => c.Id == cartId)
                .ExecuteDeleteAsync(ct);

            return new CartDTO { CartId = cartId, Items = [], GrandTotal = 0 };
        }
    }
}