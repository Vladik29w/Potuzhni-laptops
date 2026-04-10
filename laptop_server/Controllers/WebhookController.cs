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
        public WebhookController(IMonopayService payService, IOrderService orderService)
        {
            _payService = payService;
            _orderService = orderService;
        }
        [HttpPost("getWebhook")]
        public async Task<IActionResult> GetWebhook()
        {
            if (!Request.Headers.TryGetValue("X-sign", out var xSignValue))
            {
                return BadRequest();
            }
            string xSign = xSignValue.ToString();
            using var reader = new StreamReader(Request.Body);
            string body = await reader.ReadToEndAsync();

            bool Verify = await _payService.VerifyResponse(body, xSign);
            if (!Verify)
                return BadRequest();

            var data = JsonSerializer.Deserialize<MonopayWebhook>(body);
            if (data == null || !Guid.TryParse(data.Reference, out Guid orderId))
                return Ok("null or wrong guid"); //mono hoche 200


            PaymentStatus status = data.Status switch
            {
                "pending" => PaymentStatus.Pending,
                "success" => PaymentStatus.Success,
                "failure" => PaymentStatus.Failure,
                "expired" => PaymentStatus.Expired,
                "reversed" => PaymentStatus.Reversed
            };
            await _orderService.UpdateOrder(orderId, status);

            return Ok(status);
        }
    }
}
