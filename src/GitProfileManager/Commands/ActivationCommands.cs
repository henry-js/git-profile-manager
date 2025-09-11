using GitProfileManager.Lib.Services;

namespace GitProfileManager.Commands;

public class ActivationCommands(IGitConfigService service, IGitProfileStore store)
{
    /// <summary>
    /// Activates a profile
    /// </summary>
    /// <param name="profileName">PROFILE, The Git profile to activate or deactivate</param>
    /// <param name="scope">-s, Applies the profile to the specified scope, instead of the current repository</param>
    public async Task<int> Activate(
        [Argument] string profileName,
        GitConfigScope scope = GitConfigScope.Local
    )
    {
        var profile = await store.ReadProfile(profileName);
        if (profile is null)
        {
            Console.WriteLine("No profile found");
            return -1;
        }

        var tasks = profile.Select(c =>
            service.SetValueAsync(new GitConfigArgs(c.Key, c.Value, scope))
        );
        var results = await Task.WhenAll(tasks);
        if (results.All(r => r.IsSuccess))
        {
            Console.WriteLine($"All configuration from {profileName} profile applied successfully");
            return 0;
        }
        if (results.Any(r => !r.IsSuccess))
        {
            Console.WriteLine(
                "Some configuration items were not applied successfully. You may need to manually adjust your configuration"
            );
            return 1;
        }
        Console.WriteLine(
            $"Activating profile {profileName} was unsuccessful. Check that you are in a valid repository and try again!"
        );
        return 2;
    }

    /// <summary>
    /// Deactivates a profile
    /// </summary>
    /// <param name="profileName">PROFILE, The Git profile to activate or deactivate</param>
    /// <param name="scope">-s, Applies the profile to the specified scope, instead of the current repository</param>
    public async Task<int> Deactivate(
        [Argument] string profileName,
        GitConfigScope scope = GitConfigScope.Local
    )
    {
        var profile = await store.ReadProfile(profileName);
        if (profile is null)
        {
            Console.WriteLine("No profile found");
            return -1;
        }

        var tasks = profile.Select(c =>
            service.UnsetValueAsync(new GitConfigArgs(c.Key, c.Value, scope))
        );
        var results = await Task.WhenAll(tasks);

        if (results.All(r => r))
        {
            Console.WriteLine(
                $"All configuration from {profileName} profile reversed successfully"
            );
            return 0;
        }

        if (results.Any(r => r))
        {
            Console.WriteLine(
                $"Some configuration items were not reversed successfully. You may need to manually adjust your configuration"
            );
            return 1;
        }

        Console.WriteLine(
            $"Deactivating profile {profileName} was unsuccessful. Check that you are in a valid repository and try again!"
        );
        return 2;
    }
}
