using MagPie_Ewelink_Proxy.Services;
using Microsoft.AspNetCore.Mvc;

namespace MagPie_Ewelink_Proxy.Controllers
{
    [Route("[controller]/{deviceId}")]
    [ApiController]
    public class ToggleController : ControllerBase
    {
        private readonly EwelinkService _ewelinkService;

        public ToggleController(EwelinkService ewelinkService)
        {
            _ewelinkService = ewelinkService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string deviceId)
        {
            try
            {
                await _ewelinkService.Toggle(deviceId);
                return Ok("OK");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
