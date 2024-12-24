using System;
using System.Net;
using Newtonsoft.Json;

namespace CodeBase.API.Middleware;

public class ApiException : Exception
{
    public ApiException()
    {
        Code = HttpStatusCode.BadRequest;
    }

    public ApiException(object errors)
    {
        Code = HttpStatusCode.BadRequest;
        Errors = errors;
    }

    public ApiException(object errors, int errorCode, HttpStatusCode code = HttpStatusCode.BadRequest)
    {
        Code = code;
        Errors = errors;
        ErrorCode = errorCode;
    }

    public ApiException(HttpStatusCode code, object errors)
    {
        Code = code;
        Errors = errors;
    }

    public HttpStatusCode Code { get; set; }

    /// <summary>
    /// Gets or sets message code
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public object? Errors { get; set; }

    public int ErrorCode { get; set; }
}

public class InvalidArgumentException : ApiException
{
    public InvalidArgumentException(string message) : base(message, 435)
    {
    }
}