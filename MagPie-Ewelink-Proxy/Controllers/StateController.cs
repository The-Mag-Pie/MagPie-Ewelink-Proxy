using MagPie_Ewelink_Proxy.Services;
using Microsoft.AspNetCore.Mvc;

namespace MagPie_Ewelink_Proxy.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StateController : ControllerBase
    {
        private readonly EwelinkService _ewelinkService;

        public StateController(EwelinkService ewelinkService)
        {
            _ewelinkService = ewelinkService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok(await _ewelinkService.GetState());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
