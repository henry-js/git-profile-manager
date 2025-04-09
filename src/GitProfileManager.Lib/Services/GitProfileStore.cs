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
        var file = await SaveProfileManager(d);
        return file.Length > 0;
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

    private async Task<FileInfo> SaveProfileManager(ProfileManager d)
    {
        var yaml = ser.Serialize(d);
        var file = await fs.GetProfileFile();
        await fs.WriteFileAsync(file.FullName, yaml);
        file.Refresh();

        return file;
    }

    private async Task<ProfileManager> GetProfileManager()
    {
        var file = await fs.GetProfileFile();
        var content = await fs.ReadFileAsync(file.FullName);
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
    Task<string> ReadFileAsync(string path);
    Task WriteFileAsync(string path, string content);
    Task<FileInfo> GetProfileFile();
    bool FileExists();
}
