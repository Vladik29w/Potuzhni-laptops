using LaptopServer.DTO;
using LaptopServer.Enums;
using LaptopServer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaptopServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController(IOrderService orderService) : ControllerBase
    {
        [HttpPost("create")]
        public async Task<ActionResult<OrderDTO>> CreateOrder(CreateOrderDTO order)
        {
            var res = await orderService.CreateOrder(order, HttpContext.RequestAborted);
            if (res.IsError)
                return BadRequest(res.FirstError.Code);
            return Ok(res.Value);
        }
        [HttpPost("confirm/{orderId}")]
        public async Task<IActionResult> ConfirmOrder(Guid orderId)
        {
            await orderService.ConfirmOrder(orderId);
            return Ok(orderId);
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderStatsDTO>> GetOrderStats(int days, CancellationToken ct)
        {
            var result = await orderService.GetOrderStats(days, ct);
            return Ok(result);
        }
    }
}
