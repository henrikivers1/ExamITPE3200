using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GalacticSlicer.Helpers;

// Safety net for every controller action (registered in Program.cs).
// Actions that change data handle their own errors with try/catch; anything that still goes wrong
// (for example the database failing while a page is read) is logged here and the player gets the
// friendly "500" status page instead of a crash or a stack trace, in Development too.
public class LogExceptionFilter : IExceptionFilter
{
    private readonly ILogger<LogExceptionFilter> _logger;

    public LogExceptionFilter(ILogger<LogExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception,
            "[LogExceptionFilter] Unhandled error in {Action} for path {Path}",
            context.ActionDescriptor.DisplayName,
            context.HttpContext.Request.Path);

        // 500 is shown by UseStatusCodePagesWithReExecute (Home/HttpStatus)
        context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
        context.ExceptionHandled = true;
    }
}
