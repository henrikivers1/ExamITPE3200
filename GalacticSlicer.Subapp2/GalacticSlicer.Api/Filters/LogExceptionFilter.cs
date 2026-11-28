using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GalacticSlicer.Api.Filters;

// Safety net for every controller action (registered in Program.cs), like
// Helpers/LogExceptionFilter.cs in Subapp 1. Expected problems (validation,
// conflicts, not found) are handled in the controllers; anything that still
// goes wrong, for example the database failing, is logged here.
// The React client gets a 500 with a { message } it can show, never a stack trace.
public class LogExceptionFilter : IExceptionFilter
{
    private readonly ILogger<LogExceptionFilter> _logger;

    public LogExceptionFilter(ILogger<LogExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(
            context.Exception,
            "Unhandled error in {Action} for {Method} {Path}",
            context.ActionDescriptor.DisplayName,
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path);

        context.Result = new ObjectResult(new
        {
            message = "Something went wrong on the server. Please try again."
        })
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };

        context.ExceptionHandled = true;
    }
}
