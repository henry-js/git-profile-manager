using henryjs.Nuke.Components;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MinVer;
using Serilog;

class Build : NukeBuild, IAssetRelease, ITest, IHasSolution
{
    [MinVer]
    MinVer MinVer;
    public static int Main() => Execute<Build>(x => (x as IAssetRelease).Compile);

    [NuGetPackage(
        packageId: "vpk",
        packageExecutable: "vpk.dll",
        Version = "0.0.1053"
    )]
    readonly Tool Vpk;
    IAssetRelease Release => this;

    Target IAssetRelease.AssetRelease => _ => _
        .DependsOn<IPublish>(x => x.Publish)
        .OnlyWhenDynamic(Release.MainProjectIsExecutable)
        .Executes(() =>
        {
            Log.Information("Cleaning Directory: {Directory}", Release.ReleaseDirectory);

            var pkgId = Release.PackageId;
            var pubVer = Release.AssetVersion;
            var pubDir = Release.PublishDirectory;
            var mainExe = Release.AssetExecutable;
            var relDir = Release.ReleaseDirectory;
            Vpk.Invoke($"pack --packId {Release.PackageId} --packVersion {Release.AssetVersion} --packDir {Release.PublishDirectory} --mainExe {Release.AssetExecutable} --outputDir {Release.ReleaseDirectory} --shortcuts None");
        });

    Target ITest.Test => _ => _
        .Executes(() =>
        {
            var testProjects = (this as IHasSolution).TestProjects;
            foreach (var proj in testProjects)
            {
                DotNetTasks.DotNetTest(_ => _
                    .SetProjectFile(proj)
                );
                Log.Information(proj.Name);
            }
        });
}
