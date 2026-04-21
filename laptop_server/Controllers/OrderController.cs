using LaptopServer.DTO;
using LaptopServer.Enums;
using LaptopServer.Service;
using Microsoft.AspNetCore.Mvc;

namespace LaptopServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController(IOrderService orderService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<OrderDTO>> CreateOrder(CreateOrderDTO order)
        {
            var res = await orderService.CreateOrder(order, HttpContext.RequestAborted);
            if (res.IsError)
                return BadRequest(res.FirstError.Code);
            return Ok(res.Value);
        }
        [HttpPost("{orderId}")]
        public async Task<IActionResult> ConfirmOrder(Guid orderId)
        {
            await orderService.ConfirmOrder(orderId);
            return Ok(orderId);
        }
    }
}
