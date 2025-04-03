namespace GitProfileManager.Commands;

public class ProfileCommands(IGitProfileStore store)
{
    /// <summary>
    /// Lists available profiles
    /// </summary>
    /// <returns></returns>
    public async Task<int> List()
    {
        var profiles = store.GetProfiles();
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
    /// <param name="name">PROFILE, The profile name</param>
    /// <param name="from">-f, An existing profile to base the new profile on (essentially duplicates the existing profile)</param>
    public async Task<int> Create([Argument] string name, string from)
    {
        var source = !string.IsNullOrWhiteSpace(from);
        var cmds = new Dictionary<string, string>();
        if (source)
        {
            cmds = store.ReadProfile(from);
        }
        var d = store.WriteProfile(name, cmds);
        if (d)
        {
            Console.WriteLine($"Succesfully created '{name}' profile {(source ? "from " + from : string.Empty)}");
            Console.WriteLine($"Activate it using 'git-profile-manager activate {name}'");
            return 200;
        }
        Console.WriteLine($"Error encountered while creating profile!");
        return 500;
    }

    public async Task<int> Delete([Argument] string name, bool interactive = true)
    {

    }
}