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

    public SettingController(
        ILogger<SettingController> logger,
        ApplicationSettings applicationSettings,
        IQuestService questService) : base(logger)
    {
        _applicationSettings = applicationSettings;
        _questService = questService;
    }
    
    [HttpGet("get")]
    [Authorize]
    public async Task<ApplicationSettings> GetSetting()
    {
        return _applicationSettings;
    }
    
    [HttpPost("initialize")]
    public async Task<IActionResult> InitializeQuests()
    {
        var result = await _questService.InitializeQuests();

        
        // _context.Quests.AddRange(quests);
        // _context.SaveChanges();

        return Ok("Quests and milestones initialized successfully.");
    }
}