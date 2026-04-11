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
        [HttpPost]
        public async Task<ActionResult<Guid>> CreateOrder(CreateOrderDTO order)
        {
            var res = await _orderService.CreateOrder(order, HttpContext.RequestAborted);
            if (res.IsError)
                return BadRequest(res.FirstError.Description);
            return Ok(res.Value);
        }
    }
}
