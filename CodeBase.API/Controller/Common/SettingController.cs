using AutoMapper;
using CodeBase.API.Controller.Dtos;
using CodeBase.Model.Setting;
using CodeBase.QuestService;
using CodeBase.Utility.UserSession;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeBase.API.Controller;

[Route("api/setting")]
[ApiController]
public class SettingController : BaseController<SettingController>
{
    private readonly ApplicationSettings _applicationSettings;
    private readonly IQuestService _questService;
    private IUserSession _userSession;
    private readonly IMapper _mapper;

    public SettingController(
        ILogger<SettingController> logger,
        ApplicationSettings applicationSettings,
        IQuestService questService, IUserSession userSession,
        IMapper mapper) : base(logger)
    {
        _applicationSettings = applicationSettings;
        _questService = questService;
        _userSession = userSession;
        _mapper = mapper;
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
        var result = await _questService.InitializeQuestsAsync();
        return result 
            ? Ok("Quests and milestones initialized successfully.")
            : BadRequest("Failed to initialize quests and milestones.");
    }
    [HttpGet("get-quests")]
    [Authorize]
    public async Task<IActionResult> GetQuests()
    {
        if(!_userSession.IsSupperUser)
            return Unauthorized("You are not authorized to perform this action.");
        var quests = await _questService.GetAllQuestsAsync();
        var result = _mapper.Map<List<QuestDto>>(quests);
        return Ok(result);
    }
}