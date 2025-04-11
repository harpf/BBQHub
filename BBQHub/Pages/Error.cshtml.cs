using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

public class ErrorModel : PageModel
{
    private readonly ILogger<ErrorModel> _logger;

    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
    public string? Path { get; set; }

    public ErrorModel(ILogger<ErrorModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionHandlerFeature != null)
        {
            ErrorMessage = exceptionHandlerFeature.Error.Message;
            StackTrace = exceptionHandlerFeature.Error.StackTrace;
            Path = exceptionHandlerFeature.Path;

            _logger.LogError(exceptionHandlerFeature.Error,
                "Fehler aufgetreten bei {Path} (RequestId: {RequestId})", Path, RequestId);
        }
    }
}