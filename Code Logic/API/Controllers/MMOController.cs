using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MMOTool.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MMOController : ControllerBase
    {

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                isRunning = false,
                stats = new { }
            });
        }

        [HttpPost("start")]
        public IActionResult Start()
        {
            try
            {
                return Ok(new { message = "MMO Engine started successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("stop")]
        public IActionResult Stop()
        {
            try
            {
                return Ok(new { message = "MMO Engine stopped successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            return Ok(new { });
        }
    }
}
