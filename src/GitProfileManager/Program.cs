using GitProfileManager.Services;
using Velopack;

VelopackApp.Build().Run();

MyServiceProvider sp = new();

ConsoleApp.ServiceProvider = sp;
var app = ConsoleApp.Create();
app.Add<ActivationCommands>();
app.Add<ProfileCommands>("profile");

await app.RunAsync(args);

internal sealed class ReplaceLogFilter(ConsoleAppFilter next, ILogger<Program> logger)
    : ConsoleAppFilter(next)
{
    public override Task InvokeAsync(ConsoleAppContext context, CancellationToken cancellationToken)
    {
        ConsoleApp.Log = msg => logger.LogInformation(msg);
        ConsoleApp.LogError = msg => logger.LogError(msg);

        return Next.InvokeAsync(context, cancellationToken);
    }
}
