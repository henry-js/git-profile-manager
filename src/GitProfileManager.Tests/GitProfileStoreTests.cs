using AwesomeAssertions;
using GitProfileManager.Lib.Configuration;
using GitProfileManager.Lib.Services;
using NSubstitute;

namespace GitProfileManager.Tests;

public class GitProfileStoreTests
{
    private readonly IFileSystem _fileSystem;
    private readonly IProfileSerializer _serializer;

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
    public async Task ReadProfile_ReturnsExpectedProfile()
    {
        var profileManager = new ProfileManager
        {
            Profiles = new() { ["work"] = new() { ["user.name"] = "Alice" } },
        };
        const string yaml = "mock-content";

        _fileSystem.ReadFileAsync().Returns(yaml);
        _serializer.DeSerialize(yaml).Returns(profileManager);
        var sut = new GitProfileStore(_fileSystem, _serializer);

        var result = await sut.ReadProfile("work");

        result.Should().ContainKey("user.name");
        result["user.name"].Should().BeEquivalentTo("Alice");
    }

    [Test]
    public async Task ReadProfile_ReturnsNull_WhenProfileNotFound()
    {
        var emptyProfileMan = new ProfileManager() { Profiles = [] };
        const string emptyYaml = "";
        _fileSystem.ReadFileAsync().Returns(emptyYaml);
        _serializer.DeSerialize(emptyYaml).Returns(emptyProfileMan);

        var sut = new GitProfileStore(_fileSystem, _serializer);
        var result = await sut.ReadProfile("nonexistent");

        result.Should().BeNull();
    }

    [Test]
    public async Task WriteProfile_SavesToDisk()
    {
        const string yaml = "serialized-yaml";
        const string profileName = "personal";
        Dictionary<string, string> configurations = new() { ["user.email"] = "me@example.com" };
        var profileManager = new ProfileManager { Profiles = [] };
        _fileSystem.ReadFileAsync().Returns("existing-yaml");
        _serializer.DeSerialize("existing-yaml").Returns(profileManager);
        _serializer.Serialize(Arg.Any<ProfileManager>()).Returns(yaml);
        var sut = new GitProfileStore(_fileSystem, _serializer);

        var result = await sut.WriteProfile(profileName, configurations);

        await _fileSystem.Received(1).WriteFileAsync(yaml);
        result.Should().BeTrue();
        profileManager.Profiles[profileName].Should().BeEquivalentTo(configurations);
    }

    [Test]
    public async Task DeleteProfile_RemovesFromDisk()
    {
        const string yaml = "updated-yaml";
        const string profileName = "old-profile";

        var profiles = new Dictionary<string, Dictionary<string, string>>
        {
            [profileName] = new Dictionary<string, string> { ["user.name"] = "Jane" },
        };
        var manager = new ProfileManager { Profiles = profiles };

        _fileSystem.ReadFileAsync().Returns("existing-yaml");
        _serializer.DeSerialize("existing-yaml").Returns(manager);
        _serializer.Serialize(manager).Returns(yaml);

        var sut = new GitProfileStore(_fileSystem, _serializer);
        var result = await sut.DeleteProfile(profileName);

        result.Should().BeTrue();
        manager.Profiles.ContainsKey(profileName).Should().BeFalse();
        await _fileSystem.Received(1).WriteFileAsync(yaml);
    }

    [Test]
    public async Task GetProfileNames_ReturnsAllProfileKeys()
    {
        const string yaml = "some-yaml";
        var manager = new ProfileManager
        {
            Profiles = new Dictionary<string, Dictionary<string, string>>
            {
                ["work"] = [],
                ["personal"] = [],
            },
        };

        _fileSystem.ReadFileAsync().Returns(yaml);
        _serializer.DeSerialize(yaml).Returns(manager);

        var sut = new GitProfileStore(_fileSystem, _serializer);

        var result = await sut.GetProfileNames();

        result.Should().Contain("work");
        result.Should().Contain("personal");
    }
}
