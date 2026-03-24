using ErrorOr;
using LaptopServer.DTO;
using LaptopServer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaptopServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }
        [HttpGet("{CartId}")]
        public async Task<ActionResult<CartDTO>> GetCart(Guid cartId)
        {
            var cart = await _cartService.GetCart(cartId);
            return Ok(cart);
        }
        [HttpDelete("{CartId}")]
        public async Task<ActionResult<CartDTO>> ClearCart(Guid cartId)
        {
            var updCart = await _cartService.ClearCart(cartId);
            return Ok(updCart);
        }
        [HttpPost("{cartId}/{laptopId}")]
        public async Task<ActionResult<CartDTO>> AddToCart(Guid cartId, string laptopId)
        {
            var result = await _cartService.AddToCart(cartId, laptopId);
            if (result.IsError)
                return NotFound(result.FirstError.Code);
            return Ok(result.Value);
        }
        [HttpDelete("{cartId}/{laptopId}")]
        public async Task<ActionResult<CartDTO>> RemoveFromCart(Guid cartId, string laptopId)
        {
            var result = await _cartService.RemoveFromCart(cartId, laptopId);
            if (result.IsError)
                return NotFound(result.FirstError.Code);
            return Ok(result.Value);
        }
    }
}
