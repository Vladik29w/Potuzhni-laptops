using LaptopServer.DTO.NovaPost;
using LaptopServer.Infrastructure.API;
using Microsoft.AspNetCore.Mvc;

namespace LaptopServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NovaPostController(INovaPostService npService) : ControllerBase
    {
        [HttpGet("city")]
        public async Task<ActionResult<List<NpSettlementAddress>>> GetCity(string cityName, CancellationToken ct = default)
        {
            var res = await npService.GetCities(cityName, ct);
            if (res.IsError)
                return BadRequest(res.FirstError.Code);
            return Ok(res.Value);
        }
        [HttpGet("warehouse/{cityRef}")]
        public async Task<ActionResult<List<NpWarehouse>>> GetWarehouse([FromRoute] string cityRef, [FromQuery] string? searchString, CancellationToken ct = default)
        {
            var res = await npService.GetWarehouses(cityRef, searchString, ct);
            if (res.IsError)
                return BadRequest(res.FirstError.Code);
            return Ok(res.Value);
        }
    }
}