using GitProfileManager.Lib.Configuration;

namespace GitProfileManager.Lib.Services;

public class GitProfileStore : IGitProfileStore
{
    public GitProfileStore(IFileSystem fileSystem, IProfileSerializer profileSerializer)
    {
        this.fs = fileSystem;
        this.ser = profileSerializer;
    }
    private readonly IFileSystem fs;
    private readonly IProfileSerializer ser;

    public async Task<Dictionary<string, string>?> ReadProfile(string profileName)
    {
        var d = await GetProfileManager();
        return d.Profiles.TryGetValue(profileName, out var value) ? value : null;
    }

    public async Task<bool> WriteProfile(string profileName, Dictionary<string, string> configurations)
    {
        var d = await GetProfileManager();
        d.Profiles[profileName] = configurations;
        var success = await SaveProfileManager(d);
        return success;
    }

    public async Task<bool> DeleteProfile(string profileName)
    {
        var d = await GetProfileManager();
        d.Profiles.Remove(profileName);
        await SaveProfileManager(d);
        return true;
    }

    public async Task<IEnumerable<string>> GetProfileNames()
    {
        var d = await GetProfileManager();
        return d.Profiles.Keys;
    }

    private async Task<bool> SaveProfileManager(ProfileManager d)
    {
        var yaml = ser.Serialize(d);
        await fs.WriteFileAsync(yaml);
        return true;
    }

    private async Task<ProfileManager> GetProfileManager()
    {
        var content = await fs.ReadFileAsync();
        return ser.DeSerialize(content);
    }
}



public interface IProfileSerializer
{
    string Serialize(ProfileManager profileManager);
    ProfileManager DeSerialize(string content);
    ProfileManager MigrateIfNeeded(string content);
}
public interface IFileSystem
{
    Task<string> ReadFileAsync();
    Task WriteFileAsync(string content);
    bool FileExists();
}
