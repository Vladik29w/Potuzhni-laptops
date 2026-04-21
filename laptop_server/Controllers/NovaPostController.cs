using LaptopServer.DTO.NovaPost;
using LaptopServer.Infrastructure.API.NovaPost;
using Microsoft.AspNetCore.Mvc;

namespace LaptopServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NovaPostController(INovaPostApiService npApi, INovaPostDbService npDb) : ControllerBase
    {
        [HttpGet("city")]
        public async Task<ActionResult<List<NpSettlementAddress>>> GetCity(string cityName, CancellationToken ct = default)
        {
            var res = await npApi.GetCities(cityName, ct);
            if (res.IsError)
                return BadRequest(res.FirstError.Code);
            return Ok(res.Value);
        }
        [HttpGet("warehouse/{cityRef}")]
        public async Task<ActionResult<List<NpWarehouse>>> GetWarehouse([FromRoute] string cityRef, [FromQuery] string? searchString, CancellationToken ct = default)
        {
            var res = await npDb.GetWarehouses(cityRef, searchString, ct);
            return Ok(res);
        }
    }
}