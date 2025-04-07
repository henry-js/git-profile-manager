namespace GitProfileManager.Services;

public interface IGitConfigService
{
    Task<GitResult> SetValueAsync(GitConfigArgs args);
    Task<bool> UnsetValueAsync(GitConfigArgs args);
}