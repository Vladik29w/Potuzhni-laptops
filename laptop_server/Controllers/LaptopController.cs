using LaptopServer.DTO;
using LaptopServer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaptopServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LaptopController : ControllerBase
    {
        private readonly ILaptopService _laptopService = null!;
        public LaptopController(ILaptopService laptopService)
        {
            _laptopService = laptopService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllLaptops()
        {
            var laptops = await _laptopService.GetAllLaptops();
            return Ok(laptops);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<LaptopDetailsDTO>> GetById(string id)
        {
            var laptop = await _laptopService.GetById(id);
            if (laptop == null)
                return NotFound();
            else
                return Ok(laptop);
        }
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<LaptopAdminDTO>> GetLaptopsAdmin()
        {
            var laptops = await _laptopService.GetLaptopsAdmin();
            return Ok(laptops);
        }
    }
}

