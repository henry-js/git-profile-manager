using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace GitProfileManager.Services;

public class GitProfileStore : IGitProfileStore
{
    private const string _fileName = ".gitprofiles";
    public async Task<Dictionary<string, string>?> ReadProfile(string profileName)
    {
        var file = await GetProfileFile();
        var d = await GetProfiles(file);
        return d.Profiles.TryGetValue(profileName, out var value) ? value : null;
    }

    public async Task<bool> WriteProfile(string profileName, Dictionary<string, string> configurations)
    {
        var file = await GetProfileFile();
        var d = await GetProfiles(file);
        d.Profiles[profileName] = configurations;
        SaveProfiles(file, d);
        return file.Length > 0;
    }

    public async Task<bool> DeleteProfile(string profileName)
    {
        var file = await GetProfileFile();
        var d = await GetProfiles(file);
        d.Profiles.Remove(profileName);
        SaveProfiles(file, d);
        return true;
    }

    public async Task<IEnumerable<string>> GetProfileNames()
    {
        var file = await GetProfileFile();
        var d = await GetProfiles(file);
        return d.Profiles.Keys;
    }

    private static void SaveProfiles(FileInfo file, ProfileManager d)
    {
        var ser = new StaticSerializerBuilder(new YamlStaticContext()).Build();
        var yaml = ser.Serialize(d);
        File.WriteAllText(file.FullName, yaml);
        file.Refresh();
    }

    private static async Task<ProfileManager> GetProfiles(FileInfo file)
    {
        var deser = new StaticDeserializerBuilder(new YamlStaticContext()).Build();
        var content = await File.ReadAllTextAsync(file.FullName);

        return MigrateToProfileManager(content, deser);
    }

    private static ProfileManager MigrateToProfileManager(string content, IDeserializer deserializer)
    {
        try
        {
            return deserializer.Deserialize<ProfileManager>(content);
        }
        catch (YamlException)
        {
            try
            {
                var profiles = deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(content);
                return new ProfileManager() { Profiles = profiles };
            }
            catch (Exception e)
            {
                throw new MigrationException("Could not migrate legacy .gitprofiles file to new ProfileManager format", e);
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static async Task<FileInfo> GetProfileFile()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var homeDir = new DirectoryInfo(home);
        if (!homeDir.Exists) throw new DirectoryNotFoundException($"Could not locate home directory (tried {homeDir.FullName})");
        var file = new FileInfo(Path.Combine(homeDir.FullName, _fileName));
        if (!file.Exists)
        {
            await using var s = file.Create();
            await s.FlushAsync();
            await s.DisposeAsync();
        }
        file.Refresh();
        return file;
    }
}
