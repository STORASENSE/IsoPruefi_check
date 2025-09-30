using Microsoft.AspNetCore.Mvc;
using Rest_API.Services.Temp;
using Asp.Versioning;

[ApiController]
[ApiVersion("1")]
[Route("api/v{v:apiVersion}/[controller]/[action]")]
public class LocationController : ControllerBase
{
    private readonly ITempService _tempService;

    public LocationController(ITempService tempService /*, ICoordinateRepo coordinateRepo, ... */)
    {
        _tempService = tempService;
    }

    // POST /api/v1/Location/GetCoordinates/90402
    [HttpPost("{postalCode:int}")]
    public async Task<IActionResult> GetCoordinates([FromRoute] int postalCode)
    {
        await _tempService.GetCoordinates(postalCode);
        return NoContent();
    }

    // GET /api/v1/Location/ShowAvailableLocations
    [HttpGet]
    public async Task<IActionResult> ShowAvailableLocations()
    {
        var list = await _tempService.ShowAvailableLocations();
        return Ok(list ?? new List<Tuple<int,string>>());
    }
}