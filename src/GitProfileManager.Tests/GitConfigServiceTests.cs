using GitProfileManager.Services;
using NSubstitute;

namespace GitProfileManager.Tests;

public class GitConfigServiceTests
{
    private readonly IGit _git;
    private readonly GitConfigService sut;

    public GitConfigServiceTests()
    {
        _git = Substitute.For<IGit>();
        sut = new GitConfigService(_git);
    }

    [Before(Test)]
    public void SetUpGitSubstitute() => _git.ClearReceivedCalls();

    [Test]
    public async Task SetValueAsync_ShouldWrapValueInQuotes_AndCallGitConfig()
    {
        // Arrange
        _git.Config(Arg.Any<string[]>())
                .Returns(new GitResult(true, "", "", 0));

        // Act
        var args = new GitConfigArgs("user.name", "Alice");
        var result = await sut.SetValueAsync(args);

        // Assert
        await Assert.That(result.IsSuccess).IsTrue();
        await _git.Received(1).Config(Arg.Is<string[]>(args =>
            args[0] == "--local" &&
            args[1] == "set" &&
            args[2] == "user.name" &&
            args[3] == "\"Alice\""
        ));
    }
}