namespace GitProfileManager.Services;

[ServiceProvider]
[Singleton<ILoggerFactory>(Instance = nameof(LoggerFactory))]
[Singleton(typeof(ILogger<>), Factory = nameof(CreateLogger))]
[Import(typeof(IOptionsModule))]
[Transient<IConfigureOptions<CliConfig>>(Factory = nameof(BindCliConfig))]
[Singleton<IGitConfigService, GitCommandService>]
[Singleton<GitCommandRunner>]
[Transient<IGitProfileStore, FileProfileStore>]
[Singleton<ICommandFileService, CommandFileService>]
[Singleton<IService, ServiceImplementation>]
[Singleton<MyCommands>]
[Singleton<ActivationCommands>]
[Singleton<IConfiguration>(Factory = nameof(CreateConfiguration))]

internal partial class MyServiceProvider
{
    private IConfiguration CreateConfiguration()
        => new ConfigurationBuilder()
            .AddJsonFile("./config.json", false)
            .Build();

    public ILoggerFactory LoggerFactory
        => MSLogger.Create(builder => builder.AddConsole());

    private ILogger<T> CreateLogger<T>()
        => LoggerFactory.CreateLogger<T>();

    private static IConfigureOptions<CliConfig> BindCliConfig(IConfiguration configuration)
        => IOptionsModule
            .Configure<CliConfig>(config => configuration.Bind("Config", config));
}
