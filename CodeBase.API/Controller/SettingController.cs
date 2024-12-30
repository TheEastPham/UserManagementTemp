using System.Threading.Tasks;
using CodeBase.Model.Setting;
using CodeBase.Utility.UserSession;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CodeBase.API.Controller;

[Route("api/setting")]
[ApiController]
public class SettingController : BaseController<SettingController>
{
    private readonly ApplicationSettings _applicationSettings;
    private readonly IUserSession _userSession;
    public SettingController(ILogger<SettingController> logger, ApplicationSettings applicationSettings, IUserSession userSession) : base(logger)
    {
        _applicationSettings = applicationSettings;
        _userSession = userSession;
    }
    
    [HttpGet("get")]
    [Authorize]
    public async Task<ApplicationSettings> GetSetting()
    {
        return _applicationSettings;
    }
}