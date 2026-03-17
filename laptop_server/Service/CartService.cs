using LaptopServer.DB;
using LaptopServer.DTO;
using LaptopServer.Entities;
using Microsoft.EntityFrameworkCore;

namespace LaptopServer.Service
{
    public interface ICartService
    {
        Task<CartDTO> GetCart(Guid cartId);
        Task<CartDTO> AddToCart(Guid cartId, string laptopId);
        Task<CartDTO> RemoveFromCart(Guid cartId, string laptopId);
        Task<CartDTO> ClearCart(Guid cartId);
    }
    public class CartService : ICartService
    {
        private readonly LaptopsDBContext _dbContext;
        public CartService(LaptopsDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<CartDTO> GetCart(Guid cartId)
        {
            var cartItems = await _dbContext.Carts
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
                .ToListAsync();

            return new CartDTO
            {
                CartId = cartId,
                Items = cartItems,
                GrandTotal = cartItems.Sum(i => i.TotalPrice)
            };
        }
        public async Task<CartDTO> AddToCart(Guid cartId, string laptopId)
        {
            if (await _dbContext.Laptops.AnyAsync(i => i.Id == laptopId))
            {
                var cartItem = await _dbContext.Carts.FirstOrDefaultAsync(c => c.CartId == cartId && c.LaptopId == laptopId);
                if (cartItem == null)
                {
                    cartItem = new CartItemEntity
                    {
                        CartId = cartId,
                        LaptopId = laptopId,
                        Quantity = 1
                    };
                    _dbContext.Add(cartItem);
                }
                else
                    cartItem.Quantity++;
                await _dbContext.SaveChangesAsync();
                return await GetCart(cartId);
            }
            else throw new KeyNotFoundException("Laptop not found");
        }
        public async Task<CartDTO> RemoveFromCart(Guid cartId, string laptopId)
        {
            var cartItem = await _dbContext.Carts.FirstOrDefaultAsync(c => c.CartId == cartId && c.LaptopId == laptopId);
            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                    cartItem.Quantity--;
                else
                    _dbContext.Remove(cartItem);

                await _dbContext.SaveChangesAsync();
            }
            return await GetCart(cartId);
        }
        public async Task<CartDTO> ClearCart(Guid cartId)
        {
            await _dbContext.Carts
           .Where(c => c.CartId == cartId)
           .ExecuteDeleteAsync();
            return new CartDTO { CartId = cartId };
        }
    }
}
