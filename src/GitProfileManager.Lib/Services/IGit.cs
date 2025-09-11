using CliWrap;
using CliWrap.Buffered;
using CliWrap.Exceptions;

namespace GitProfileManager.Lib.Services;

public class Git : IGit
{
    private readonly Command _git;
    private const string GIT = "git";
    private const string CONFIG = "config";
    private const string VERSION = "--version";

    public Git() => _git = Cli.Wrap(GIT)
    // .WithValidation(CommandResultValidation.None)
    ;

    public async Task<GitResult> Config(params string[] args)
    {
        try
        {
            var result = await _git.WithArguments([CONFIG, .. args]).ExecuteBufferedAsync();

            return new GitResult(
                result.IsSuccess,
                result.StandardOutput,
                result.StandardError,
                result.ExitCode
            );
        }
        catch (CommandExecutionException ex)
        {
            throw new GitCommandFailedException(ex.Message, ex);
        }
    }

    public async Task<Version> Version()
    {
        var result = await _git.WithArguments(VERSION).ExecuteBufferedAsync();
        var versionString = result.StandardOutput.Split("version").Last().Trim();

        var parts = versionString.Split('.').Take(3).Select(x => int.Parse(x)).ToArray();
        return new Version(parts[0], parts[1], parts[2]);
    }
}

[Serializable]
public class GitCommandFailedException : Exception
{
    public GitCommandFailedException() { }

    public GitCommandFailedException(string? message)
        : base(message) { }

    public GitCommandFailedException(string? message, Exception? innerException)
        : base(message, innerException) { }
}

public interface IGit
{
    Task<GitResult> Config(params string[] args);
    Task<Version> Version();
}

public record GitResult(bool IsSuccess, string StdOut, string StdErr, int ExitCode);
