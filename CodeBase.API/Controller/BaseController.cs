using System.Collections.Generic;
using System.Linq;
using CodeBase.API.Extension;
using CodeBase.Model.TransferObject.UserData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CodeBase.API.Controller;

[ApiController]
public abstract class BaseController<TController> : ControllerBase
{
    protected ILogger Logger { get; }

    public BaseController(ILogger<TController> logger)
    {
        Logger = logger;
    }

    private UserDataDto? _userIdentity;

    public UserDataDto? UserIdentity => _userIdentity ??= Request.Headers.GetUserIdentify();

    public bool IsAuthorized => UserIdentity != null;

    protected Dictionary<string, string> GetHeaders()
    {
        return HttpContext.Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString());
    }
}