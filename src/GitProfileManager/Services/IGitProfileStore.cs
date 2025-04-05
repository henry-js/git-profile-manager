namespace GitProfileManager.Services;

public interface IGitProfileStore
{
    Task<IEnumerable<string>> GetProfiles();
    Task<Dictionary<string, string>?> ReadProfile(string profileName);
    Task<bool> WriteProfile(string profileName, Dictionary<string, string> configurations);
    Task<bool> DeleteProfile(string profileName);
}