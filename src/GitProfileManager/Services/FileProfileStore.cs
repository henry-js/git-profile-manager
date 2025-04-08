using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace GitProfileManager.Services;

public class GitProfileStore : IGitProfileStore
{
    private const string _fileName = ".gitprofiles";
    public async Task<Dictionary<string, string>?> ReadProfile(string profileName)
    {
        var file = await GetProfileFile();
        var d = await GetProfiles(file);
        return d.TryGetValue(profileName, out var value) ? value : null;
    }

    public async Task<bool> WriteProfile(string profileName, Dictionary<string, string> configurations)
    {
        var file = await GetProfileFile();
        var d = await GetProfiles(file);
        d[profileName] = configurations;
        SaveProfiles(file, d);
        return file.Length > 0;
    }

    public async Task<bool> DeleteProfile(string profileName)
    {
        var file = await GetProfileFile();
        var d = await GetProfiles(file);
        d.Remove(profileName);
        SaveProfiles(file, d);
        return true;
    }

    public async Task<IEnumerable<string>> GetProfiles()
    {
        var file = await GetProfileFile();
        var d = await GetProfiles(file);
        return d.Keys;
    }

    private static void SaveProfiles(FileInfo file, Dictionary<string, Dictionary<string, string>> d)
    {
        var ser = new StaticSerializerBuilder(new YamlStaticContext()).Build();
        var yaml = ser.Serialize(d);
        File.WriteAllText(file.FullName, yaml);
        file.Refresh();
    }

    private static async Task<Dictionary<string, Dictionary<string, string>>> GetProfiles(FileInfo file)
    {
        var deser = new StaticDeserializerBuilder(new YamlStaticContext()).Build();
        var content = await File.ReadAllTextAsync(file.FullName);
        var d = deser.Deserialize<Dictionary<string, Dictionary<string, string>>>(content);
        return d ?? [];
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

[YamlStaticContext]
public partial class YamlStaticContext : StaticContext;