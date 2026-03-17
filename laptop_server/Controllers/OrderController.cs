using LaptopServer.Enums;
using LaptopServer.Service;
using Microsoft.AspNetCore.Mvc;

namespace LaptopServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        public record CheckoutRequest(Guid cartId, PayEnum pay, DeliveryEnum delivery, string phone, string email, string address);
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateOrder([FromBody] CheckoutRequest request)
        {
            var order = await _orderService.CreateOrder
                (request.cartId, request.pay, request.delivery, request.phone, request.email, request.address, HttpContext.RequestAborted);
            return Ok(order);
        }
    }
}
