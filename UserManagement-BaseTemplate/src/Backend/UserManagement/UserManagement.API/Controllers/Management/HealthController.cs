using Microsoft.AspNetCore.Mvc;

namespace UserManagement.API.Controllers.Management;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet]
    public ActionResult GetHealth()
    {
        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            service = "UserManagement.API",
            version = "1.0.0"
        });
    }
}
