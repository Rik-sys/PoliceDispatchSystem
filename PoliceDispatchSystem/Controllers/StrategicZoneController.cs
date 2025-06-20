using IBL;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class StrategicZoneController : ControllerBase
{
    private readonly IStrategicZoneBL _bl;

    public StrategicZoneController(IStrategicZoneBL bl)
    {
        _bl = bl;
    }

    [HttpGet("all")]
    public IActionResult GetAll()
    {
        try
        {
            var zones = _bl.GetAllStrategicZones();
            return Ok(zones);
        }
        catch (Exception ex)
        {
            return BadRequest($"שגיאה: {ex.Message}");
        }
    }
}
