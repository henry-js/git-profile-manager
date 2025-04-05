namespace GitProfileManager.Services;

[ServiceProvider]
[Singleton<ILoggerFactory>(Instance = nameof(LoggerFactory))]
[Singleton(typeof(ILogger<>), Factory = nameof(CreateLogger))]
[Import(typeof(IOptionsModule))]
[Transient<IConfigureOptions<CliConfig>>(Factory = nameof(BindCliConfig))]
[Singleton<IGit, Git>]
[Singleton<IGitConfigService, GitConfigService>]
[Transient<IGitProfileStore, FileProfileStore>]
[Singleton<ICommandFileService, CommandFileService>]
[Singleton<ActivationCommands>]
[Singleton<ProfileCommands>]
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
