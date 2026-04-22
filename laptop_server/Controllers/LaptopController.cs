using LaptopServer.DTO;
using LaptopServer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaptopServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LaptopController(ILaptopService laptopService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllLaptops(CancellationToken ct)
        {
            var laptops = await laptopService.GetAllLaptops(ct);
            return Ok(laptops);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<LaptopDetailsDTO>> GetById(Guid id, CancellationToken ct)
        {
            var laptop = await laptopService.GetById(id, ct);
            if (laptop == null)
                return NotFound();
            else
                return Ok(laptop);
        }
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<LaptopAdminDTO>> GetLaptopsAdmin(CancellationToken ct)
        {
            var laptops = await laptopService.GetLaptopsAdmin(ct);
            return Ok(laptops);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddLaptop([FromBody] LaptopAdminDTO laptop, CancellationToken ct)
        {
            var result = await laptopService.AddLaptop(laptop, ct);
            if (result.IsError)
                return BadRequest(result.FirstError.Code);
            return CreatedAtAction(nameof(AddLaptop), new { id = result.Value.Id }, result.Value);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateLaptop([FromBody] LaptopAdminDTO laptop, CancellationToken ct)
        {
            var result = await laptopService.UpdateLaptop(laptop, ct);
            if (result.IsError)
                return BadRequest(result.FirstError.Code);
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteLaptop(Guid id, CancellationToken ct)
        {
            var result = await laptopService.DeleteLaptop(id, ct);
            if (result.IsError)
                return BadRequest(result.FirstError.Code);
            return NoContent();
        }
    }
}

