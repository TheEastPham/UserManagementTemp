using System.Threading.Tasks;
using CodeBase.Model.Setting;
using CodeBase.QuestService;
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
    private readonly IQuestService _questService;
    private IUserSession _userSession;

    public SettingController(
        ILogger<SettingController> logger,
        ApplicationSettings applicationSettings,
        IQuestService questService, IUserSession userSession) : base(logger)
    {
        _applicationSettings = applicationSettings;
        _questService = questService;
        _userSession = userSession;
    }
    
    [HttpGet("get")]
    [Authorize]
    public async Task<ApplicationSettings> GetSetting()
    {
        return _applicationSettings;
    }
    
    [HttpPost("initialize")]
    [Authorize]
    public async Task<IActionResult> InitializeQuests()
    {
        if(!_userSession.IsSupperUser)
            return Unauthorized("You are not authorized to perform this action.");
        var result = await _questService.InitializeQuests();
        return result 
            ? Ok("Quests and milestones initialized successfully.")
            : BadRequest("Failed to initialize quests and milestones.");
    }
}