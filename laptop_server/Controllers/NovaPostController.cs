using LaptopServer.DTO.NovaPost;
using LaptopServer.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;

namespace LaptopServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NovaPostController : ControllerBase
    {
        private readonly INovaPostService _npService;

        public NovaPostController(INovaPostService npService)
        {
            _npService = npService;
        }
        [HttpGet("city")]
        public async Task<ActionResult<List<NpSettlementAddress>>> GetCity(string cityName)
        {
            var res = await _npService.GetCities(cityName);
            if (res.IsError)
                return BadRequest(res.FirstError.Code);
            return Ok(res.Value);
        }
        [HttpGet("warehouse/{cityRef}")]
        public async Task<ActionResult<List<NpWarehouse>>> GetWarehouse([FromRoute] string cityRef, [FromQuery] string? searchString)
        {
            var res = await _npService.GetWarehouses(cityRef, searchString);
            if (res.IsError)
                return BadRequest(res.FirstError.Code);
            return Ok(res.Value);
        }
    }
}