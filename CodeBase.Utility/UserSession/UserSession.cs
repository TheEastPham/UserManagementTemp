using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CodeBase.Utility.UserSession;

public class UserSession : IUserSession
{
    private ClaimsPrincipal Principal => _httpContextAccessor.HttpContext?.User;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserSession(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public string ClientId => Principal.FindFirst("client_id")?.Value ?? string.Empty;

    public string Email => Principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
    private string? Sid => Principal?.FindFirst(ClaimTypes.Sid)?.Value;
    public Guid UserSid => Sid != null ? Guid.Parse(Sid) : Guid.Empty;

    public string Name => Principal?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
}