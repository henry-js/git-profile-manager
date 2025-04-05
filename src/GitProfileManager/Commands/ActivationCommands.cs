namespace GitProfileManager.Commands;

public class ActivationCommands(IGitConfigService service, IGitProfileStore store)
{
    /// <summary>
    /// Activates a profile
    /// </summary>
    /// <param name="profileName">PROFILE, The Git profile to activate or deactivate</param>
    /// <param name="global">-g, Applies the profile globally, instead of the current repository</param>
    public async Task<int> Activate([Argument] string profileName, bool global = false)
    {
        var profile = await store.ReadProfile(profileName);
        if (profile is null)
        {
            Console.WriteLine("No profile found");
            return -1;
        }

        var tasks = profile.Select(c => service.SetValueAsync(c.Key, c.Value, global));
        var results = await Task.WhenAll(tasks);
        if (results.All(r => r))
        {
            Console.WriteLine($"All configuration from {profileName} profile applied successfully");
            return 0;
        }
        if (results.Any(r => r))
        {
            Console.WriteLine($"Some configuration items were not applied successfully. You may need to manually adjust your configuration");
            return 1;
        }
        Console.WriteLine($"Activating profile {profileName} was unsuccessful. Check that you are in a valid repository and try again!");
        return 2;
    }

    /// <summary>
    /// Deactivates a profile
    /// </summary>
    /// <param name="profileName">PROFILE, The Git profile to activate or deactivate</param>
    /// <param name="global">-g, Applies the profile globally, instead of the current repository</param>
    public async Task<int> Deactivate([Argument] string profileName, bool global = false)
    {
        var profile = await store.ReadProfile(profileName);
        if (profile is null)
        {
            Console.WriteLine("No profile found");
            return -1;
        }

        var tasks = profile.Select(c => service.UnsetValueAsync(c.Key, c.Value, global));
        var results = await Task.WhenAll(tasks);
        if (results.All(r => r))
        {
            Console.WriteLine($"All configuration from {profileName} profile reversed successfully");
            return 0;
        }
        if (results.Any(r => r))
        {
            Console.WriteLine($"Some configuration items were not reversed successfully. You may need to manually adjust your configuration");
            return 1;
        }
        Console.WriteLine($"Deactivating profile {profileName} was unsuccessful. Check that you are in a valid repository and try again!");
        return 2;
    }
}
