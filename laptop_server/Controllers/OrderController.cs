using LaptopServer.DTO;
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
        public record CheckoutRequest(
            Guid CartId,
            PayEnum PayMethod,
            DeliveryEnum DeliveryMethod,
            string PhoneNumber,
            string? Email,
            string? ShippingAddress
        );
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateOrder([FromBody] CreateOrderDTO request)
        {
            var orderId = await _orderService.CreateOrder(request, HttpContext.RequestAborted);
            return Ok(orderId);
        }
    }
}
