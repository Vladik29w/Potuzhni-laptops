using LaptopServer.DTO;
using LaptopServer.Service;
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
            var laptop = await _laptopService.GetAllLaptops();
            return Ok(laptop);
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
    }
}

