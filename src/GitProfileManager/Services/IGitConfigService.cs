namespace GitProfileManager.Services;

public interface IGitConfigService
{
    Task<bool> SetValueAsync(string key, string value, bool global = false);
    Task<bool> UnsetValueAsync(string key, string value, bool global = false);
}