
using GitProfileManager.Lib.Configuration;
using GitProfileManager.Lib.Services;

namespace GitProfileManager.Services;

[ServiceProvider]
[Singleton<ILoggerFactory>(Instance = nameof(LoggerFactory))]
[Singleton(typeof(ILogger<>), Factory = nameof(CreateLogger))]
[Import(typeof(IOptionsModule))]
// [Transient<IConfigureOptions<CliConfig>>(Factory = nameof(BindCliConfig))]
[Singleton<IGit, Git>]
[Singleton<IGitConfigService, GitConfigService>]
[Transient<IGitProfileStore, GitProfileStore>]
[Singleton<IFileSystem>(Instance = nameof(CreateFileManager))]
[Singleton<IProfileSerializer, YamlProfileSerializer>]
[Singleton<ICommandFileService, CommandFileService>]
[Singleton<ActivationCommands>]
[Singleton<ProfileCommands>]
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
        => MSLogger.Create(builder => builder.AddConsole());

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