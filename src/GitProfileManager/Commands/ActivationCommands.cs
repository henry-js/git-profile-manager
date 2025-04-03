using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GitProfileManager.Services;
using Spectre.Cli;

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
        var profile = store.ReadProfile(profileName);
        var list = new List<bool>();
        var results = profile.Select(c => service.SetValue(c.Key, c.Value, global)).ToList();
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
        var profile = store.ReadProfile(profileName);
        var list = new List<bool>();
        var results = profile.Select(c => service.UnsetValue(c.Key, c.Value, global)).ToList();
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
