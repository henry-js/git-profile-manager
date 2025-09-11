using GitProfileManager.Exceptions;
using GitProfileManager.Lib.Configuration;
using GitProfileManager.Lib.Services;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GitProfileManager.Services;

[YamlStaticContext]
[YamlSerializable(typeof(ProfileManager))]
public partial class YamlStaticContext : StaticContext;

public class YamlProfileSerializer : IProfileSerializer
{
    private readonly IDeserializer _deser;
    private readonly ISerializer _ser;

    public YamlProfileSerializer()
    {
        _deser = new StaticDeserializerBuilder(new YamlStaticContext())
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        _ser = new StaticSerializerBuilder(new YamlStaticContext())
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    public ProfileManager DeSerialize(string content)
    {
        return MigrateIfNeeded(content);
    }

    public string Serialize(ProfileManager profileManager)
    {
        return _ser.Serialize(profileManager);
    }

    public ProfileManager MigrateIfNeeded(string content)
    {
        try
        {
            return _deser.Deserialize<ProfileManager>(content);
        }
        catch (YamlException)
        {
            try
            {
                var profiles = _deser.Deserialize<Dictionary<string, Dictionary<string, string>>>(
                    content
                );
                return new ProfileManager() { Profiles = profiles };
            }
            catch (Exception e)
            {
                throw new MigrationException(
                    "Could not migrate legacy .gitprofiles file to new ProfileManager format",
                    e
                );
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
}
