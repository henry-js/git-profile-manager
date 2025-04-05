using CliWrap;

namespace GitProfileManager.Services;
public class Git : IGit
{
    private readonly Command _git;

    public Git()
    {
        _git = Cli.Wrap("git");
    }
    public async Task<CommandResult> Config(params string[] args)
    {
        return await _git.WithArguments(args)
            .ExecuteAsync();
    }
}

public interface IGit
{
    Task<CommandResult> Config(params string[] args);
}