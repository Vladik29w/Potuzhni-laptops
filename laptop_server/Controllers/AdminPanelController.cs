using LaptopServer.DTO;
using LaptopServer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaptopServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminPanelController : ControllerBase
    {
        private readonly IAdminPanelService _adminPanelService;
        public AdminPanelController(IAdminPanelService adminPanelService)
        {
            _adminPanelService = adminPanelService;
        }
        [HttpPost("laptop")]
        public async Task<IActionResult> AddLaptop([FromBody] LaptopDetailsDTO laptop)
        {
            var result = await _adminPanelService.AddLaptop(laptop);
            if (result.IsError)
                return BadRequest(result.FirstError.Code);
            return CreatedAtAction(nameof(AddLaptop), new { id = result.Value.Id }, result.Value);
        }
        [HttpPut("laptop")]
        public async Task<IActionResult> UpdateLaptop([FromBody] LaptopDetailsDTO laptop)
        {
            var result = await _adminPanelService.UpdateLaptop(laptop);
            if (result.IsError)
                return BadRequest(result.FirstError.Code);
            return Ok();
        }
        [HttpDelete("laptop/{id}")]
        public async Task<IActionResult> DeleteLaptop(string id)
        {
            var result = await _adminPanelService.DeleteLaptop(id);
            if (result.IsError)
                return BadRequest(result.FirstError.Code);
            return NoContent();
        }
    }
}
