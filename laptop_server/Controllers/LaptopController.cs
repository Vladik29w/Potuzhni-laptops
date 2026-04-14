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
    }
}

