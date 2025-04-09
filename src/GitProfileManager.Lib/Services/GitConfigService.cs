
namespace GitProfileManager.Lib.Services;

public class GitConfigService : IGitConfigService
{
    private readonly IGit git;

    public GitConfigService(IGit git) => this.git = git;

    public async Task<GitResult> SetValueAsync(GitConfigArgs args)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(args.Key);
        ArgumentException.ThrowIfNullOrWhiteSpace(args.Value);

        if (!args.Value.StartsWith('\"'))
        {
            args = args with { Value = $"\"{args.Value}\"" };
        }
        var scope = GetScope(args.Scope);
        var result = await git.Config([scope, "set", args.Key, args.Value]);

        return result;
    }

    public async Task<bool> UnsetValueAsync(GitConfigArgs args)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(args.Key);

        var scope = GetScope(args.Scope);

        var result = await git.Config([scope, "unset", args.Key]);

        return result.IsSuccess;
        // var output = _runner.RunCommand($"config {(global ? "--global" : string.Empty)} --unset {key} {value}");
        // return output.Item1 == 0 && output.Item2 == string.Empty;
    }

    private static string GetScope(GitConfigScope scope)
    {
        return scope switch
        {
            GitConfigScope.Local => "--local",
            GitConfigScope.Global => "--global",
            GitConfigScope.Unknown => throw new NotImplementedException(),
            _ => throw new NotImplementedException(),
        };
    }
}

public enum GitConfigScope { Unknown = 0,   /* Worktree = 1, */    Local = 2, Global = 3,  /*  System = 4, */};
public record GitConfigArgs(string Key, string? Value, GitConfigScope Scope = GitConfigScope.Local);
