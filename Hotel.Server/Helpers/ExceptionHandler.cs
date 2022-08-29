using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;

namespace Hotel.Server.Helpers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorsController : ControllerBase
{
    [Route("error")]
    public ErrorResponse HandleError()
    {
        var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var exception = context?.Error;
        var code = HttpStatusCode.InternalServerError;

        if (exception is ConflictException) code = HttpStatusCode.Conflict;
        else if (exception is NotFoundException) code = HttpStatusCode.NotFound;
        else if (exception is UnauthorizedException) code = HttpStatusCode.Unauthorized;
        else if (exception is BadRequestException) code = HttpStatusCode.BadRequest;
        else if (exception is ForbiddenException) code = HttpStatusCode.Forbidden;
        else if (exception is AuthNetException) code = HttpStatusCode.InternalServerError;

        Response.StatusCode = (int)code;

        return new ErrorResponse(exception, Response.StatusCode);
    }
}

public class ErrorResponse
{
    public ErrorResponse(Exception e, int status)
    {
        Title = e.Message;
        Status = status;
    }

    public string Title { get; set; }
    public int Status { get; set; }
}
