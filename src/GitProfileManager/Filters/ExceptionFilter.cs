using Spectre.Console;

namespace GitProfileManager.Filters;

internal sealed class ExceptionFilter(ConsoleAppFilter next, ILoggerFactory factory)
    : ConsoleAppFilter(next)
{
    public override async Task InvokeAsync(ConsoleAppContext context, CancellationToken cancellationToken)
    {
        var logger = factory.CreateLogger("Program");

        try
        {
            await Next.InvokeAsync(context, cancellationToken);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]{ex.Message}[/]");
            logger.LogError(ex, "Program stopped");
        }
    }
}