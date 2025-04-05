using System;

namespace GitProfileManager.Services
{
    public class GitConfigService : IGitConfigService
    {
        private readonly IGit git;

        public GitConfigService(IGit git) => this.git = git;

        public async Task<bool> SetValueAsync(string key, string value, bool global = false)
        {
            if (!value.StartsWith("\""))
            {
                value = $"\"{value}\"";
            }

            var result = await git.Config([(global ? "--global" : string.Empty), "set", key, value]);

            return result.IsSuccess;
            // var output = _runner.RunCommand($"config {(global ? "--global" : string.Empty)} {key} {value}");
            // return output.Item1 == 0 && output.Item2 == string.Empty;
        }

        public async Task<bool> UnsetValueAsync(string key, string value, bool global = false)
        {
            if (!value.StartsWith("\""))
            {
                value = $"\"{value}\"";
            }

            var result = await git.Config([(global ? "--global" : string.Empty), "unset", key]);

            return result.IsSuccess;
            // var output = _runner.RunCommand($"config {(global ? "--global" : string.Empty)} --unset {key} {value}");
            // return output.Item1 == 0 && output.Item2 == string.Empty;
        }
    }
}