using FluentAssertions;
using GitProfileManager.Lib.Services;

namespace GitProfileManager.Tests.Integration;

[Explicit]
public class GitTests
{
    private readonly IGit _git;

    public GitTests() => _git = new Git();
    [Test]
    public async Task Version_Should_ReturnExitCodeZero_ForGitVersion()
    {
        var result = await _git.Version();

        result.Should().NotBeNull();
    }

    [Test]
    public async Task Version_Should_Return_Valid_Version()
    {
        // Act
        var version = await _git.Version();

        // Assert
        version.Major.Should().BeGreaterThanOrEqualTo(1); // Any realistic Git install
        version.Minor.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    public async Task Config_List_Should_Return_Config_Lines()
    {
        // Act
        var result = await _git.Config("--list");

        // Assert
        result.ExitCode.Should().Be(0);
        result.StdOut.Should().NotBeNullOrWhiteSpace();
        result.StdOut.Should().Contain("user.");
    }

    [Test]
    public async Task Config_Get_Should_Return_Global_UserName()
    {
        // Act
        var result = await _git.Config("--global", "user.name");

        // Assert
        result.ExitCode.Should().Be(0);
        result.StdOut.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task Config_Set_NonexistentKey_Should_ThrowException()
    {
        // Act
        var act = () => _git.Config("--global", "set", "this.key.does.not.exist", "abc");

        // Assert
        await act.Should().ThrowAsync<GitCommandFailedException>();
    }

    [Test]
    public async Task Config_Unset_NonexistentKey_Should_ThrowException()
    {
        // Act
        var act = () => _git.Config("--local", "unset", "this.key.does.not.exist");

        // Assert
        await act.Should().ThrowAsync<GitCommandFailedException>();
    }
}
