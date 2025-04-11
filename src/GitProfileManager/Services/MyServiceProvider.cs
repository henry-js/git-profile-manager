
using GitProfileManager.Lib.Configuration;
using GitProfileManager.Lib.Services;
using Serilog;

namespace GitProfileManager.Services;

[ServiceProvider]
[Singleton<ILoggerFactory>(Instance = nameof(LoggerFactory))]
[Singleton(typeof(ILogger<>), Factory = nameof(CreateLogger))]
[Import(typeof(IOptionsModule))]
[Singleton<IGit, Git>]
[Singleton<IGitConfigService, GitConfigService>]
[Transient<IGitProfileStore, GitProfileStore>]
[Singleton<IFileSystem>(Instance = nameof(CreateFileManager))]
[Singleton<IProfileSerializer, YamlProfileSerializer>]
[Singleton<ICommandFileService, CommandFileService>]
[Singleton<ActivationCommands>]
[Singleton<ProfileCommands>]
// [Transient<IConfigureOptions<CliConfig>>(Factory = nameof(BindCliConfig))]
// [Singleton<IConfiguration>(Factory = nameof(CreateConfiguration))]

internal partial class MyServiceProvider
{
    // private IConfiguration CreateConfiguration()
    //     => new ConfigurationBuilder()
    //         .AddJsonFile("./config.json", false)
    //         .Build();

    private IFileSystem CreateFileManager =>
        new FileManager(
            Environment.GetEnvironmentVariable("GPM_PROFILE_PATH", EnvironmentVariableTarget.User)
            ?? (XDGHelper.GetConfigFilePath())
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ProfileManager.FileName));
    private ILoggerFactory LoggerFactory
        => MSLogger.Create(builder => builder.AddSerilog(
            new LoggerConfiguration()
                    .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "application.log"),
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u}] {SourceContext}: {Message:lj}{NewLine}{Exception}",
                        rollingInterval: RollingInterval.Day,
                        shared: true)
                    .Enrich.WithProperty("Application Name", "<APP NAME>")
                .CreateLogger()));

    private ILogger<T> CreateLogger<T>()
        => LoggerFactory.CreateLogger<T>();

    // private static IConfigureOptions<CliConfig> BindCliConfig(IConfiguration configuration)
    //     => IOptionsModule
    //         .Configure<CliConfig>(config => configuration.Bind("Config", config));
}

public static class XDGHelper
{
    private static string? CONFIG_HOME => Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
    public static string? GetConfigFilePath()
    {
        var result = CONFIG_HOME is null ? null : Path.Combine(CONFIG_HOME, "git-profile-manager", ProfileManager.FileName);
        var dir = Path.GetDirectoryName(result);
        if (!Directory.Exists(dir) && dir is not null) Directory.CreateDirectory(dir);
        return result;
    }
}