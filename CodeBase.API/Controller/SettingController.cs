using System.Threading.Tasks;
using CodeBase.Model.Setting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CodeBase.API.Controller;

[Route("api/setting")]
[ApiController]
public class SettingController : BaseController<SettingController>
{
    private readonly ApplicationSettings _applicationSettings;
    public SettingController(ILogger<SettingController> logger, ApplicationSettings applicationSettings) : base(logger)
    {
        _applicationSettings = applicationSettings;
    }
    
    [HttpGet("get")]
    public async Task<ApplicationSettings> GetSetting()
    {
        return _applicationSettings;
    }
}