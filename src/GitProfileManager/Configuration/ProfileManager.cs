using YamlDotNet.Serialization;

namespace GitProfileManager.Configuration;

public class ProfileManager
{
    public Dictionary<string, Dictionary<string, string>> Profiles { get; set; } = [];
}
[YamlStaticContext]
[YamlSerializable(typeof(ProfileManager))]
public partial class YamlStaticContext : StaticContext
{
}