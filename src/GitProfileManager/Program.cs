using GitProfileManager.Filters;
using GitProfileManager.Services;

using Velopack;

VelopackApp.Build().Run();

// #if !DEBUG
EnvironmentHelper.AddToPath();
// #endif
MyServiceProvider sp = new();

ConsoleApp.ServiceProvider = sp;
var app = ConsoleApp.Create();

app.Add<ActivationCommands>();
app.Add<ProfileCommands>("profile");
app.UseFilter<ExceptionFilter>();

// inject logger to filter

await app.RunAsync(args);

#if DEBUG
Console.WriteLine("Press enter to exit");
Console.ReadLine();
#endif

public static class EnvironmentHelper
{
    public static void AddToPath()
    {
        string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? string.Empty;
        string appDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
        appDirectory = path.EndsWith(Path.PathSeparator) ? appDirectory : $"{Path.PathSeparator}{appDirectory}";

        if (path.Contains(appDirectory))
        {
            return;
        }
        else
        {
            Environment.SetEnvironmentVariable("PATH", appDirectory, EnvironmentVariableTarget.User);
        }

        Console.WriteLine($"BaseDirectory: {appDirectory}");
        Console.WriteLine($"CurrentDirectory: {Directory.GetCurrentDirectory()}");
    }
}