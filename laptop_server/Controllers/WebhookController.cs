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
    public class WebhookController(IMonopayService payService, IOrderService orderService, ILogger<WebhookController> logger) : ControllerBase
    {
        [HttpPost("getWebhook")]
        public async Task<IActionResult> GetWebhook(CancellationToken ct)
        {
            if (!Request.Headers.TryGetValue("X-sign", out var xSign))
                return BadRequest("Null xsign");
            using var reader = new StreamReader(Request.Body);
            string body = await reader.ReadToEndAsync(ct);
            var res = await payService.CheckWebhook(body, xSign!, ct);
            if (res.IsError)
                return Problem(res.FirstError.Code);
            logger.LogInformation($"Get webhook for {res.Value.orderId}");
            await orderService.UpdateOrder(res.Value.orderId, res.Value.status, ct);

            return res.Match<IActionResult>(
                Success => Ok(),
                err => Problem(err[0].Code)
                );
        }
    }
}
