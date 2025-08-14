using System;
using System.IO.Compression;
using System.IO;
using System.Linq;
using Microsoft.Build.Tasks;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using System.Text;
using Serilog;
using Nuke.Common.CI.GitHubActions;

[GitHubActions(
    "Nuke Build",
    GitHubActionsImage.WindowsLatest,
    On = new[] { GitHubActionsTrigger.WorkflowDispatch },
    InvokedTargets = new[] { nameof(BuildAll) })]
public partial class Build : NukeBuild
{
    private static string ThisVersion = "1.6.0";
    public static int Main() => Execute<Build>(x => x.BuildAll);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Net runtime for compilation for specific target.")]
    readonly NetRuntime NetRuntime = NetRuntime.None;

    [GitRepository]
    readonly GitRepository Repository;

    AbsolutePath SourceDirectory => RootDirectory / "src" / "Src";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath ArtifactsTmpDirectory => RootDirectory / "artifacts" / ".tmp";

    [NuGetPackage(
        packageId: "nuget-license",
        packageExecutable: "NuGetLicenseCore.dll",
        Framework = "net8.0")]
    readonly Tool DotnetProjectLicenses;

    Target Clean => _ => _
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(t => t.DeleteDirectory());
            //TODO: delete native directories

            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target BuildBouncyHsm => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            AbsolutePath projectFile = SourceDirectory / "BouncyHsm" / "BouncyHsm.csproj";
            AbsolutePath outputDir = ArtifactsTmpDirectory / "BouncyHsm";

            DotNetPublish(_ => _
            .SetConfiguration(Configuration)
            .AddProperty("GitCommit", Repository.Commit)
            .When(NetRuntime != NetRuntime.None, q => q.SetRuntime(NetRuntime))
            .SetProject(projectFile)
            .SetOutput(outputDir));

            CopyLicenses(projectFile, outputDir);
        });

    Target BuildBouncyHsmCli => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            AbsolutePath projectFile = SourceDirectory / "BouncyHsm.Cli" / "BouncyHsm.Cli.csproj";
            AbsolutePath outputDir = ArtifactsTmpDirectory / "BouncyHsm.Cli";

            DotNetPublish(_ => _
            .SetConfiguration(Configuration)
            .AddProperty("GitCommit", Repository.Commit)
            .When(NetRuntime != NetRuntime.None, q => q.SetRuntime(NetRuntime))
            .SetProject(projectFile)
            .SetOutput(outputDir));

            CopyLicenses(projectFile, outputDir);
        });

    Target BuildBouncyHsmClient => _ => _
       .DependsOn(Clean)
       .DependsOn(BuildPkcs11LibWin32)
       .DependsOn(BuildPkcs11LibX64)
       .Executes(() =>
       {
           AbsolutePath linuxNativeLibx64 = RootDirectory / "build_linux" / "BouncyHsm.Pkcs11Lib-x64.so";
           if (linuxNativeLibx64.Exists("file"))
           {
               linuxNativeLibx64.Copy(ArtifactsTmpDirectory / "native" / "Linux-x64" / "BouncyHsm.Pkcs11Lib.so", ExistsPolicy.FileOverwrite);
           }

           AbsolutePath rhelNativeLibx64 = RootDirectory / "build_linux" / "BouncyHsm.Pkcs11Lib-x64-rhel.so";
           if (rhelNativeLibx64.Exists("file"))
           {
               rhelNativeLibx64.Copy(ArtifactsTmpDirectory / "native" / "Rhel-x64" / "BouncyHsm.Pkcs11Lib.so", ExistsPolicy.FileOverwrite);
           }

           AbsolutePath projectFile = SourceDirectory / "BouncyHsm.Client" / "BouncyHsm.Client.csproj";
           AbsolutePath outputDir = ArtifactsDirectory;

           DotNetPack(_ => _
           .SetConfiguration(Configuration)
           .AddProperty("RepositoryCommit", Repository.Commit)
           .AddProperty("RepositoryBranch", Repository.Branch)
           .AddProperty("IncludeNativeLibs", "True")
           .When(NetRuntime != NetRuntime.None, q => q.SetRuntime(NetRuntime))
           .SetProject(projectFile)
           .SetOutputDirectory(outputDir));
       });

    Target BuildAll => _ => _
        .DependsOn(Clean)
        .DependsOn(BuildPkcs11LibWin32)
        .DependsOn(BuildPkcs11LibX64)
        .DependsOn(BuildBouncyHsm)
        .DependsOn(BuildBouncyHsmCli)
        .DependsOn(BuildBouncyHsmClient)
        .Produces(ArtifactsDirectory / "*.zip")
        .Executes(() =>
        {
            (ArtifactsTmpDirectory / "native").Copy(ArtifactsTmpDirectory / "BouncyHsm" / "native");
            CreateZip(ArtifactsTmpDirectory / "native" / "Win-x64" / "BouncyHsm.Pkcs11Lib.dll",
                "Win X64",
                ThisVersion,
                ArtifactsTmpDirectory / "BouncyHsm" / "wwwroot" / "native" / "BouncyHsm.Pkcs11Lib-Winx64.zip");
            CreateZip(ArtifactsTmpDirectory / "native" / "Win-x86" / "BouncyHsm.Pkcs11Lib.dll",
                "Win X86",
                ThisVersion,
                ArtifactsTmpDirectory / "BouncyHsm" / "wwwroot" / "native" / "BouncyHsm.Pkcs11Lib-Winx86.zip");

            AbsolutePath linuxNativeLibx64 = RootDirectory / "build_linux" / "BouncyHsm.Pkcs11Lib-x64.so";
            if (linuxNativeLibx64.Exists("file"))
            {
                linuxNativeLibx64.Copy(ArtifactsTmpDirectory / "BouncyHsm" / "native" / "Linux-x64" / "BouncyHsm.Pkcs11Lib.so", ExistsPolicy.FileOverwrite);
                CreateZip(linuxNativeLibx64,
                "Linux X64",
                ThisVersion,
                ArtifactsTmpDirectory / "BouncyHsm" / "wwwroot" / "native" / "BouncyHsm.Pkcs11Lib-Linuxx64.zip");
            }
            else
            {
                Log.Warning("Native lib {0} not found.", linuxNativeLibx64);
            }

            AbsolutePath linuxNativeLibx32 = RootDirectory / "build_linux" / "BouncyHsm.Pkcs11Lib-x86.so";
            if (linuxNativeLibx32.Exists("file"))
            {
                linuxNativeLibx32.Copy(ArtifactsTmpDirectory / "BouncyHsm" / "native" / "Linux-x86" / "BouncyHsm.Pkcs11Lib.so", ExistsPolicy.FileOverwrite);

                CreateZip(linuxNativeLibx32,
               "Linux X86",
               ThisVersion,
               ArtifactsTmpDirectory / "BouncyHsm" / "wwwroot" / "native" / "BouncyHsm.Pkcs11Lib-Linuxx84.zip");
            }
            else
            {
                Log.Warning("Native lib {0} not found.", linuxNativeLibx32);
            }

            AbsolutePath rhelNativeLibx64 = RootDirectory / "build_linux" / "BouncyHsm.Pkcs11Lib-x64-rhel.so";
            if (rhelNativeLibx64.Exists("file"))
            {
                rhelNativeLibx64.Copy(ArtifactsTmpDirectory / "BouncyHsm" / "native" / "Rhel-x64" / "BouncyHsm.Pkcs11Lib.so", ExistsPolicy.FileOverwrite);
                CreateZip(rhelNativeLibx64,
                "RHEL X64",
                ThisVersion,
                ArtifactsTmpDirectory / "BouncyHsm" / "wwwroot" / "native" / "BouncyHsm.Pkcs11Lib-RHELx64.zip");
            }
            else
            {
                Log.Warning("Native lib {0} not found.", rhelNativeLibx64);
            }

            (ArtifactsTmpDirectory / "BouncyHsm" / "data" / "keep.txt").TouchFile();

            (ArtifactsTmpDirectory / "BouncyHsm").ZipTo(ArtifactsDirectory / "BouncyHsm.zip",
                t => t.Extension != ".pdb" && t.Name != "libman.json" && t.Name != ".gitkeep" && t.Name != "appsettings.Development.json");

            (ArtifactsTmpDirectory / "BouncyHsm.Cli").ZipTo(ArtifactsDirectory / "BouncyHsm.Cli.zip",
               t => t.Extension != ".pdb" && t.Name != ".gitkeep");
        });

    private void CopyLicenses(AbsolutePath csprojProjectFile, AbsolutePath outFolder)
    {
        Log.Debug("Copy license files");
        (RootDirectory / "LICENSE").Copy(outFolder / "License.txt");

        try
        {
            AbsolutePath licensesFilePath = outFolder / "LicensesThirdParty.txt";
            DotnetProjectLicenses($"--include-transitive --input \"{csprojProjectFile}\" -o Table --file-output \"{licensesFilePath}\" -f net8.0");
        }
        catch (ProcessException ex) when (ex.ExitCode == 3) // Workeround
        {
            Log.Warning(ex, "DotnetProjectLicenses exited with code {0}", ex.ExitCode);
        }
    }

    private void CreateZip(AbsolutePath dllFile, string platform, string version, AbsolutePath destination)
    {
        Log.Debug("Creating ZIP file from dll {0}", dllFile);

        using FileStream fs = new FileStream(destination, FileMode.Create);
        using ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Create);
        archive.CreateEntryFromFile(dllFile, dllFile.Name, CompressionLevel.Optimal);
        ZipArchiveEntry zipArchiveEntry = archive.CreateEntry("Readme.txt");
        using Stream readmeStream = zipArchiveEntry.Open();

        byte[] content = Encoding.UTF8.GetBytes(@$"Bouncy Hsm PKCS11 library

Version: {version}
For platform: {platform}
Git commit: {Repository.Commit}

Project site: https://github.com/harrison314/BouncyHsm
License: BSD 3 Clausule
");

        readmeStream.Write(content);
        readmeStream.Flush();
    }
}
