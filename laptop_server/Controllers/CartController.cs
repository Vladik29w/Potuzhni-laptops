using ErrorOr;
using LaptopServer.DTO;
using LaptopServer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaptopServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartController(ICartService cartService) : ControllerBase
    {
        [HttpGet("{CartId}")]
        public async Task<ActionResult<CartDTO>> GetCart(Guid cartId, CancellationToken ct)
        {
            var cart = await cartService.GetCart(cartId, ct);
            return Ok(cart);
        }
        [HttpDelete("{CartId}")]
        public async Task<ActionResult<CartDTO>> ClearCart(Guid cartId, CancellationToken ct)
        {
            var updCart = await cartService.ClearCart(cartId, ct);
            return Ok(updCart);
        }
        [HttpPost("{cartId}/{laptopId}")]
        public async Task<ActionResult<CartDTO>> AddToCart(Guid cartId, Guid laptopId, CancellationToken ct)
        {
            var result = await cartService.AddToCart(cartId, laptopId, ct);
            if (result.IsError)
                return NotFound(result.FirstError.Code);
            return Ok(result.Value);
        }
        [HttpDelete("{cartId}/{laptopId}")]
        public async Task<ActionResult<CartDTO>> RemoveFromCart(Guid cartId, Guid laptopId, CancellationToken ct)
        {
            var result = await cartService.RemoveFromCart(cartId, laptopId, ct);
            if (result.IsError)
                return NotFound(result.FirstError.Code);
            return Ok(result.Value);
        }
    }
}
