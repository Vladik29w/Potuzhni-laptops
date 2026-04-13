using ErrorOr;
using LaptopServer.DTO;
using LaptopServer.Enums;
using LaptopServer.Infrastructure.API;
using LaptopServer.Service;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using System.Text.Json;
namespace LaptopServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IMonopayService _payService;
        private readonly IOrderService _orderService;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(IMonopayService payService, IOrderService orderService, ILogger<WebhookController> logger)
        {
            _payService = payService;
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost("getWebhook")]
        public async Task<IActionResult> GetWebhook()
        {
            if (!Request.Headers.TryGetValue("X-sign", out var xSign))
                return BadRequest("Null xsign");
            using var reader = new StreamReader(Request.Body);
            string body = await reader.ReadToEndAsync();
            var res = await _payService.CheckWebhook(body, xSign!);

            return res.Match<IActionResult>(
                Success => Ok(),
                err => Problem(err[0].Code)
                );
        }
    }
}
