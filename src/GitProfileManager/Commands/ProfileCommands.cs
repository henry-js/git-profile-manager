using GitProfileManager.Lib.Services;

namespace GitProfileManager.Commands;

[ConsoleAppFilter<ReplaceLogFilter>]
public class ProfileCommands(IGitProfileStore store, ICommandFileService fileService, ILogger<ProfileCommands> logger)
{
    /// <summary>
    /// Lists available profiles
    /// </summary>
    public async Task<int> List()
    {
        logger.LogInformation("In list command");
        var profiles = await store.GetProfileNames();
        Console.WriteLine("Currently stored profiles: ");
        if (profiles.Any())
        {
            foreach (var profile in profiles)
            {
                Console.WriteLine($"- {profile}");
            }
        }
        else
        {
            Console.WriteLine(" <none>");
        }
        return 0;
    }

    /// <summary>
    /// Create a new (blank) profile
    /// </summary>
    /// <param name="name">PROFILE | The profile name</param>
    /// <param name="from">-f, An existing profile to base the new profile on (essentially duplicates the existing profile)</param>
    public async Task<int> Create([Argument] string name, string? from = null)
    {
        var source = !string.IsNullOrWhiteSpace(from);
        var cmds = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(from))
        {
            cmds = await store.ReadProfile(from);
        }
        if (cmds is null)
        {
            Console.WriteLine("No profile found");
            return -1;
        }
        var d = await store.WriteProfile(name, cmds);
        if (d)
        {
            Console.WriteLine($"Succesfully created '{name}' profile {(source ? "from " + from : string.Empty)}");
            Console.WriteLine($"Activate it using 'git-profile-manager activate {name}'");
            return 200;
        }
        Console.WriteLine($"Error encountered while creating profile!");
        return 500;
    }

    /// <summary>
    /// Delete an existing saved profile
    /// </summary>
    /// <param name="name">The profile name</param>
    /// <param name="NonInteractive">Do not prompt for user input or confirmations.</param>
    public async Task<int> Delete([Argument] string name, bool NonInteractive)
    {
        var profile = store.ReadProfile(name);
        if (profile == null) return 404;
        bool confirm = false;
        if (!NonInteractive)
        {
            while (!confirm)
            {
                Console.Write($"{Environment.NewLine}This will complete remove the '{name}' profile. Are you sure? [y/n]");
                int key;
                try
                {
                    var keyInfo = Console.ReadKey();
                    //Console.Write(keyInfo.KeyChar);
                    key = ((int)keyInfo.KeyChar);
                }
                catch (InvalidOperationException)
                {
                    key = Console.Read();
                }
                catch
                {
                    Console.WriteLine();
                    Console.Error.WriteLine("Could not confirm on current terminal. Try passing --non-interactive to delete without confirmation.");
                    return 412;
                }
                Console.WriteLine();
                if (key == 'n' || key == 'N') return 3;
                confirm = key == 'y' || key == 'Y';
            }
        }
        var del = await store.DeleteProfile(name);
        if (del)
        {
            Console.WriteLine($"Removed '{name}' profile from store!");
            return 0;
        }
        Console.Error.WriteLine("Error deleting profile from store!");
        return 1;
    }

    /// <summary>
    /// Edit an existing profile to add or remove configuration items
    /// </summary>
    /// <param name="profileName">The profile to add a new configuration to. Will be created if it does not exist</param>
    /// <param name="configVal">The config value to add to the profile, separated by an '=' symbol.</param>
    /// <param name="remove">-r, Removes the given configuration item from the profile, instead of adding it.</param>
    public async Task<int> Edit([Argument] string profileName, [Argument] string configVal, bool remove)
    {
        var profile = await store.ReadProfile(profileName) ?? [];
        var config = configVal.Split('=');
        if (profile.ContainsKey(config[0]))
        {
            profile.Remove(config[0]);
        }
        if (!remove)
        {
            profile.Add(config[0], config[1]);
        }
        var result = await store.WriteProfile(profileName, profile);
        return result ? 0 : 2;
    }

    /// <summary>
    /// Export a saved profile to a command file
    /// </summary>
    /// <param name="profileName">The profile name</param>
    /// <param name="filePath">The path to export the profile to</param>
    public async Task<int> Export([Argument] string profileName, [Argument] string filePath)
    {
        if (string.IsNullOrWhiteSpace(profileName))
        {
            Console.Error.WriteLine("No profile name provided!");
            return 400;
        }
        if (string.IsNullOrWhiteSpace(filePath))
        {
            Console.Error.WriteLine("No export path provided!");
            return 400;
        }
        var profile = await store.ReadProfile(profileName);
        if (profile == null)
        {
            Console.WriteLine($"Could not find profile '{profileName}'! Does it exist?");
            return 404;
        }
        var fi = new FileInfo(filePath);
        if (fi.Exists && fi.IsReadOnly)
        {
            Console.Error.WriteLine($"Cannot write to the file at '{fi.FullName}'!");
            return 403;
        }
        var d = fileService.WriteToFile(profile, fi, includeCommand: true);
        if (d)
        {
            Console.WriteLine($"Wrote '{profileName}' profile to command file at {fi.FullName}");
            return 0;
        }
        Console.Error.WriteLine("Error writing profile to command file!");
        return 1;
    }

    /// <summary>
    /// Import a new profile from a command file or existing config
    /// </summary>
    /// <param name="commandFile">A file of git commands to create a profile from.</param>
    /// <param name="profileName">Name of the profile to create. Defaults to the input file name</param>
    /// <param name="fromConfig">Read commands from config, rather than command file</param>
    public async Task<int> Import([Argument] string commandFile, string profileName, bool fromConfig)
    {
        if (fromConfig)
        {
            Console.WriteLine("Sorry, this functionality is not yet available :(");
            return 501;
        }
        var file = new FileInfo(commandFile);
        if (!file.Exists)
        {
            Console.Error.WriteLine($"Could not find file at {commandFile}");
            return 404;
        }
        var name = GetProfileName(profileName, commandFile);
        var cmds = fileService.ReadFromFile(file);
        var d = await store.WriteProfile(name, cmds);
        if (d)
        {
            Console.WriteLine($"Succesfully created '{name}' profile from '{file.FullName}'!");
            Console.WriteLine($"Activate it using 'git-profile-manager activate {name}'");
            return 200;
        }
        Console.WriteLine($"Error encountered while creating profile from '{file.FullName}'!");
        return 500;

        static string GetProfileName(string profileName, string file)
        {
            if (string.IsNullOrWhiteSpace(profileName))
            {
                var fi = new FileInfo(file);
                return fi.Name.Replace(fi.Extension, string.Empty).Trim().Trim('.');
            }
            return profileName;
        }
    }

    /// <summary>
    /// Show configuration from a saved profile
    /// </summary>
    /// <param name="profileName">The profile name</param>
    public async Task<int> Show([Argument] string profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName))
        {
            Console.Error.WriteLine("No profile name provided!");
            return 400;
        }
        var profile = await store.ReadProfile(profileName);
        if (profile == null)
        {
            Console.WriteLine($"Could not find profile '{profileName}'! Does it exist?");
            return 404;
        }
        Console.WriteLine($"Profile '{profileName}':");
        if (profile.Any())
        {
            foreach (var config in profile)
            {
                Console.WriteLine($"- {config.Key}={config.Value}");
            }
        }
        else
        {
            Console.WriteLine(" <none>");
        }
        return 0;
    }
}