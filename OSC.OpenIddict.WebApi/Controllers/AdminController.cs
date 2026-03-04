using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OSC.OpenIddict.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")] // This locks the whole controller to Admins
    public class AdminController : ControllerBase
    {
        [HttpGet("secret-stats")]
        public IActionResult GetStats()
        {
            return Ok(new { 
                Message = "If you can see this, you are an Admin.",
                ServerTime = DateTime.UtcNow,
                SecretCode = "RANDOM-SECRET-9999"
            });
        }
    }
}
