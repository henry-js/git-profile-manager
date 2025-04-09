using System.Numerics;
using GitProfileManager.Lib.Configuration;
using GitProfileManager.Lib.Services;
using NSubstitute;
using YamlDotNet.Serialization;

namespace GitProfileManager.Tests;

public class GitProfileStoreTests
{
    private readonly IFileSystem _fileSystem;
    private readonly IProfileSerializer _serializer;
    private readonly FileInfo _fakeFile = new("testfile");
    public GitProfileStoreTests()
    {
        _fileSystem = Substitute.For<IFileSystem>();
        _serializer = Substitute.For<IProfileSerializer>();
    }

    [Before(Test)]
    public void ClearSubstitutes()
    {
        _fileSystem.ClearReceivedCalls();
        _serializer.ClearReceivedCalls();
    }

    [Test]
    public async Task ReadProfile_ReturnsExpectedProfile_WhenExists()
    {
        var profileData = new ProfileManager()
        {
            Profiles = {
                ["work"] = { ["user.name"] = "Alice"}
            }
        };

        var sut = CreateStoreWith(profileData);
    }

    private GitProfileStore CreateStoreWith(ProfileManager profileData)
    {
        _fileSystem.GetProfileFile().Returns(_fakeFile);
    }
}