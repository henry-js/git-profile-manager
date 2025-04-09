namespace GitProfileManager.Lib.Configuration;

public class ProfileManager
{
    public const string FileName = ".gitprofiles";
    public Dictionary<string, Dictionary<string, string>> Profiles { get; set; } = [];
}
