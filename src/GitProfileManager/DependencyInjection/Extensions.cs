using GitProfileManager.Lib.Configuration;
using GitProfileManager.Lib.Services;
using GitProfileManager.Services;

using Microsoft.Extensions.DependencyInjection;

using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog.Templates;

namespace GitProfileManager.DependencyInjection;

public static class Extensions
{
    public static IConfiguration CreateConfiguration()
        => new ConfigurationBuilder()
            .AddJsonFile("./config.json", false)
            .Build();

    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddSingleton(CreateConfiguration());
        services.AddSingleton<IFileSystem, FileManager>((services) => new FileManager(
            Environment.GetEnvironmentVariable("GPM_PROFILE_PATH", EnvironmentVariableTarget.User)
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ProfileManager.FileName)
        ));
        services.AddSingleton<IProfileSerializer, YamlProfileSerializer>();
        services.AddSingleton<ICommandFileService, CommandFileService>();
        services.AddSingleton<ActivationCommands>();
        services.AddSingleton<ProfileCommands>();
        services.AddTransient<IGitProfileStore, GitProfileStore>();
        services.AddSingleton<IGit, Git>();
        services.AddSingleton<IGitConfigService, GitConfigService>();

        return services;
    }
    public static void ConfigureSerilog(this ILoggingBuilder builder)
    {
        builder.AddSerilog(
                   new LoggerConfiguration()
                           .WriteTo.File(
                               formatter: new ExpressionTemplate(
                                   "[{@t:HH:mm:ss} {@l:u3}] {@m}\n{@x}"),
                                   Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "app-.log"),
                               shared: true,
                               rollingInterval: RollingInterval.Day)
                           .Enrich.WithProperty("Application Name", "<APP NAME>")
                       .WriteTo.Console(theme: AnsiConsoleTheme.Sixteen)
                       .CreateLogger());
    }
}