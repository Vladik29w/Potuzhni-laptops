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
            _logger.LogInformation("Webhook request received");

            if (!Request.Headers.TryGetValue("X-sign", out var xSignValue))
            {
                _logger.LogWarning("X-sign header is missing from webhook request");
                return BadRequest();
            }
            string xSign = xSignValue.ToString();
            
            using var reader = new StreamReader(Request.Body);
            string body = await reader.ReadToEndAsync();
            _logger.LogDebug("Webhook body read successfully");

            bool Verify = await _payService.VerifyResponse(body, xSign);
            if (!Verify)
            {
                _logger.LogWarning("Webhook signature verification failed");
                return BadRequest();
            }
            _logger.LogInformation("Webhook signature verified successfully");

            var data = JsonSerializer.Deserialize<MonopayWebhook>(body);
            if (data == null || !Guid.TryParse(data.Reference, out Guid orderId))
            {
                _logger.LogWarning($"Invalid webhook data: null or wrong guid format. Reference: {data?.Reference}");
                return Ok("Null or wrong guid");
            }
            _logger.LogInformation("Webhook data parsed successfully. OrderId: {OrderId}", orderId);

            PaymentStatus status = data.Status switch
            {
                "pending" => PaymentStatus.Pending,
                "success" => PaymentStatus.Success,
                "failure" => PaymentStatus.Failure,
                "expired" => PaymentStatus.Expired,
                "reversed" => PaymentStatus.Reversed
            };
            _logger.LogInformation($"Payment status updated. OrderId: {orderId}, Status: {status}", orderId, status);
            
            await _orderService.UpdateOrder(orderId, status);

            return Ok(status);
        }
    }
}
