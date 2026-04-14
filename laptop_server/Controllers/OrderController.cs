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
                return BadRequest(res.FirstError.Description);
            return Ok(res.Value);
        }
    }
}
