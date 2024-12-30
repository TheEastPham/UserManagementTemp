using Microsoft.AspNetCore.Mvc;

namespace CodeBase.API.Controller;

[ApiController]
public abstract class BaseController<TController> : ControllerBase
{
    protected ILogger Logger { get; }

    public BaseController(ILogger<TController> logger)
    {
        Logger = logger;
    }
}