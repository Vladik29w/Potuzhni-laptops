using LaptopServer.DTO;
using LaptopServer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaptopServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminPanelController(IAdminPanelService adminPanelService) : ControllerBase
    {
        [HttpPost("laptop")]
        public async Task<IActionResult> AddLaptop([FromBody] LaptopAdminDTO laptop, CancellationToken ct)
        {
            var result = await adminPanelService.AddLaptop(laptop, ct);
            if (result.IsError)
                return BadRequest(result.FirstError.Code);
            return CreatedAtAction(nameof(AddLaptop), new { id = result.Value.Id }, result.Value);
        }
        [HttpPut("laptop")]
        public async Task<IActionResult> UpdateLaptop([FromBody] LaptopAdminDTO laptop, CancellationToken ct)
        {
            var result = await adminPanelService.UpdateLaptop(laptop, ct);
            if (result.IsError)
                return BadRequest(result.FirstError.Code);
            return Ok();
        }
        [HttpDelete("laptop/{id}")]
        public async Task<IActionResult> DeleteLaptop(Guid id, CancellationToken ct)
        {
            var result = await adminPanelService.DeleteLaptop(id, ct);
            if (result.IsError)
                return BadRequest(result.FirstError.Code);
            return NoContent();
        }
        [HttpGet("stats")]
        public async Task<ActionResult<OrderStatsDTO>> GetOrderStats(int days, CancellationToken ct)
        {
            var result = await adminPanelService.GetOrderStats(days, ct);
            return Ok(result);
        }
    }
}
