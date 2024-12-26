using CodeBase.Utility.JWT;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CodeBase.Identity.Controllers.Token;

[Route("api/[controller]")]
[ApiController]
public class TokenController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;

    public TokenController(UserManager<IdentityUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost]
    [Route("generate")]
    public async Task<IActionResult> GenerateToken([FromBody] LoginDto model)
    {
        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            return Unauthorized();

        var token = JwtToken.GenerateToken(user, _configuration["Jwt:Key"]);
        return Ok(new { Token = token });
    }
}