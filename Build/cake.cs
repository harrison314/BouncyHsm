using Build;
using System.IO.Compression;

string target = Argument("target", "Default");
string configuration = Argument("Configuration", "Release");

string SourceDirectory = "./src/Src/";
string ArtifactsDirectory = "./artifacts";
string ArtifactsTmpDirectory = "./artifacts/.tmp/";

GitCommit gitTip = GitLogTip(".");
GitBranch gitBranchObj = GitBranchCurrent(".");

string gitCommit = gitTip.Sha;
string gitBranch = gitBranchObj.CanonicalName;
string ThisVersion = XmlPeek("src/Directory.Build.props", "//Version/text()");

Task(BuildTarget.RebuildDocumentation)
    .Does(() =>
    {
        DotNetRun("./src/Tools/BouncyHsm.DocGenerator/BouncyHsm.DocGenerator.csproj",
            new ProcessArgumentBuilder().Append("Doc/SupportedAlgorithms.md"),
            new DotNetRunSettings()
            {
                Configuration = configuration,
                MSBuildSettings = new DotNetMSBuildSettings()
                {
                    Properties =
                {
                    {"GitCommit", new List<string>() { gitCommit } }
                }
                },
               // WorkingDirectory = $"src/Tools/BouncyHsm.DocGenerator",
               DiagnosticOutput = true
            });
    });

Task(BuildTarget.Clean)
    .Does(() =>
    {
        DeleteDirectories(GetDirectories("./src/Src/**/obj/*"), new DeleteDirectorySettings()
        {
            Recursive = true,
            Force = true
        });

        DeleteDirectories(GetDirectories("./src/Src/**/bin/*"), new DeleteDirectorySettings()
        {
            Recursive = true,
            Force = true
        });

        CleanDirectory(ArtifactsDirectory);
    });

Task(BuildTarget.BuildBouncyHsm)
    .IsDependentOn(BuildTarget.Clean)
    .Does(() =>
    {
        string projectFile = $"{SourceDirectory}BouncyHsm/BouncyHsm.csproj";
        string outputDir = $"{ArtifactsTmpDirectory}BouncyHsm";

        DotNetPublishSettings settings = new DotNetPublishSettings()
        {
            Configuration = configuration,
            OutputDirectory = outputDir,
            MSBuildSettings = new DotNetMSBuildSettings(),
        };

        settings.MSBuildSettings.Properties.Add("GitCommit", new List<string>() { gitCommit });

        DotNetPublish(projectFile, settings);
    });

Task(BuildTarget.BuildBouncyHsmCli)
    .IsDependentOn(BuildTarget.Clean)
    .Does(() =>
    {
        string projectFile = $"{SourceDirectory}BouncyHsm.Cli/BouncyHsm.Cli.csproj";
        string outputDir = $"{ArtifactsTmpDirectory}BouncyHsm.Cli";

        DotNetPublishSettings settings = new DotNetPublishSettings()
        {
            Configuration = configuration,
            OutputDirectory = outputDir,
            MSBuildSettings = new DotNetMSBuildSettings(),
        };

        settings.MSBuildSettings.Properties.Add("GitCommit", new List<string>() { gitCommit });

        DotNetPublish(projectFile, settings);
    });

void BuildBouncyHsmPkcs11Lib(PlatformTarget platform)
{
    MSBuildSettings settings = new MSBuildSettings()
    {
        Verbosity = Verbosity.Diagnostic,
        Configuration = configuration,
        PlatformTarget = platform,
        Targets =
        {
            "clean",
            "build"
        }
    };

    //TODO: Fix leather
    // Fix problem with Cake and visual studio 2026
    settings.ToolPath = @"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe";

    MSBuild($"{SourceDirectory}BouncyHsm.Pkcs11Lib/BouncyHsm.Pkcs11Lib.vcxproj", settings);
}

Task(BuildTarget.BuildPkcs11LibWin32)
    .IsDependentOn(BuildTarget.Clean)
    .Does(() =>
    {
        BuildBouncyHsmPkcs11Lib(PlatformTarget.Win32);

        string nativeLib = $"{SourceDirectory}BouncyHsm.Pkcs11Lib/{configuration}/BouncyHsm.Pkcs11Lib.dll";
        string destination = $"{ArtifactsTmpDirectory}/native/Win-x86";
        CleanDirectory(destination);
        CopyFile(nativeLib, $"{destination}/BouncyHsm.Pkcs11Lib.dll");

    });

Task(BuildTarget.BuildPkcs11LibX64)
    .IsDependentOn(BuildTarget.Clean)
    .Does(() =>
    {
        BuildBouncyHsmPkcs11Lib(PlatformTarget.x64);

        string nativeLib = $"{SourceDirectory}BouncyHsm.Pkcs11Lib/x64/{configuration}/BouncyHsm.Pkcs11Lib.dll";
        string destination = $"{ArtifactsTmpDirectory}/native/Win-x64";
        CleanDirectory(destination);
        CopyFile(nativeLib, $"{destination}/BouncyHsm.Pkcs11Lib.dll");
    });

Task(BuildTarget.BuildBouncyHsmClient)
    .IsDependentOn(BuildTarget.Clean)
    .IsDependentOn(BuildTarget.BuildPkcs11LibWin32)
    .IsDependentOn(BuildTarget.BuildPkcs11LibX64)
    .Does(() =>
    {
        string projectFile = $"{SourceDirectory}BouncyHsm.Client/BouncyHsm.Client.csproj";

        string linuxNativeLibx64 = JoinPaths("build_linux", "BouncyHsm.Pkcs11Lib-x64.so");
        if (FileExists(linuxNativeLibx64))
        {
            CopyFile(linuxNativeLibx64,
                JoinPaths(ArtifactsTmpDirectory, "native", "Linux-x64", "BouncyHsm.Pkcs11Lib.so"));
        }

        string rhelNativeLibx64 = JoinPaths("build_linux", "BouncyHsm.Pkcs11Lib-x64-rhel.so");
        if (FileExists(rhelNativeLibx64))
        {
            CopyFile(rhelNativeLibx64,
                JoinPaths(ArtifactsTmpDirectory, "native", "Rhel-x64", "BouncyHsm.Pkcs11Lib.so"));
        }


        DotNetPackSettings settings = new DotNetPackSettings()
        {
            Configuration = configuration,
            OutputDirectory = ArtifactsDirectory,
            MSBuildSettings = new DotNetMSBuildSettings(),
        };

        settings.MSBuildSettings.Properties.Add("RepositoryCommit", new List<string>() { gitCommit });
        settings.MSBuildSettings.Properties.Add("RepositoryBranch", new List<string>() { gitBranch });
        settings.MSBuildSettings.Properties.Add("IncludeNativeLibs", new List<string>() { "True" });

        DotNetPack(projectFile, settings);
    });

Task(BuildTarget.BuildAll)
    .IsDependentOn(BuildTarget.Clean)
    .IsDependentOn(BuildTarget.BuildPkcs11LibWin32)
    .IsDependentOn(BuildTarget.BuildPkcs11LibX64)
    .IsDependentOn(BuildTarget.BuildBouncyHsm)
    .IsDependentOn(BuildTarget.BuildBouncyHsmCli)
    .IsDependentOn(BuildTarget.BuildBouncyHsmClient)
    .Does(() =>
    {
        CopyDirectory($"{ArtifactsTmpDirectory}native", $"{ArtifactsTmpDirectory}BouncyHsm/native");

        CreateZip(JoinPaths(ArtifactsTmpDirectory, "native/Win-x64/BouncyHsm.Pkcs11Lib.dll"),
            "Win X64",
            ThisVersion,
            JoinPaths(ArtifactsTmpDirectory, "BouncyHsm/wwwroot/native/BouncyHsm.Pkcs11Lib-Winx64.zip"));

        CreateZip(JoinPaths(ArtifactsTmpDirectory, "native/Win-x86/BouncyHsm.Pkcs11Lib.dll"),
            "Win X64",
            ThisVersion,
            JoinPaths(ArtifactsTmpDirectory, "BouncyHsm/wwwroot/native/BouncyHsm.Pkcs11Lib-Winx86.zip"));


        string linuxNativeLibx64 = JoinPaths("build_linux", "BouncyHsm.Pkcs11Lib-x64.so");
        if (FileExists(linuxNativeLibx64))
        {
            CopyFile(linuxNativeLibx64,
                JoinPaths(ArtifactsTmpDirectory, "BouncyHsm", "native", "Linux-x64", "BouncyHsm.Pkcs11Lib.so"));

            CreateZip(linuxNativeLibx64,
                       "Linux X64",
                       ThisVersion,
                       JoinPaths(ArtifactsTmpDirectory, "BouncyHsm", "wwwroot", "native", "BouncyHsm.Pkcs11Lib-Linuxx64.zip"));
        }
        else
        {
            Warning("Native lib {0} not found.", linuxNativeLibx64);
        }

        string linuxNativeLibx32 = JoinPaths("build_linux", "BouncyHsm.Pkcs11Lib-x86.so");
        if (FileExists(linuxNativeLibx32))
        {
            CopyFile(linuxNativeLibx32,
                JoinPaths(ArtifactsTmpDirectory, "BouncyHsm", "native", "Linux-x86", "BouncyHsm.Pkcs11Lib.so"));

            CreateZip(linuxNativeLibx32,
                       "Linux X64",
                       ThisVersion,
                       JoinPaths(ArtifactsTmpDirectory, "BouncyHsm", "wwwroot", "native", "BouncyHsm.Pkcs11Lib-Linuxx86.zip"));
        }
        else
        {
            Warning("Native lib {0} not found.", linuxNativeLibx32);
        }


        string rhelNativeLibx64 = JoinPaths("build_linux", "BouncyHsm.Pkcs11Lib-x64-rhel.so");
        if (FileExists(rhelNativeLibx64))
        {
            CopyFile(rhelNativeLibx64,
                JoinPaths(ArtifactsTmpDirectory, "BouncyHsm", "native", "Rhel-x64", "BouncyHsm.Pkcs11Lib.so"));

            CreateZip(rhelNativeLibx64,
                       "RHEL X64",
                       ThisVersion,
                       JoinPaths(ArtifactsTmpDirectory, "BouncyHsm", "wwwroot", "native", "BouncyHsm.Pkcs11Lib-RHELx64.zip"));
        }
        else
        {
            Warning("Native lib {0} not found.", rhelNativeLibx64);
        }

        System.IO.Directory.CreateDirectory(JoinPaths(ArtifactsTmpDirectory, "BouncyHsm/data"));
        System.IO.File.WriteAllText(JoinPaths(ArtifactsTmpDirectory, "BouncyHsm/data/keep.txt"), string.Empty);

        DeleteFiles(JoinPaths(ArtifactsTmpDirectory, "BouncyHsm/**/*.pdb"));
        DeleteFiles(JoinPaths(ArtifactsTmpDirectory, "BouncyHsm/**/libman.json"));
        DeleteFiles(JoinPaths(ArtifactsTmpDirectory, "BouncyHsm/**/.gitkeep"));
        DeleteFiles(JoinPaths(ArtifactsTmpDirectory, "BouncyHsm/**/appsettings.Development.json"));

        Zip(JoinPaths(ArtifactsTmpDirectory, "BouncyHsm"), JoinPaths(ArtifactsDirectory, "BouncyHsm.zip"));

        DeleteFiles(JoinPaths(ArtifactsTmpDirectory, "BouncyHsm.Cli/**/*.pdb"));
        DeleteFiles(JoinPaths(ArtifactsTmpDirectory, "BouncyHsm.Cli/**/.gitkeep"));

        Zip(JoinPaths(ArtifactsTmpDirectory, "BouncyHsm.Cli"), JoinPaths(ArtifactsDirectory, "BouncyHsm.Cli.zip"));
    });


void CreateZip(string dllFile, string platform, string version, string destination)
{
    Debug("Creating ZIP file from dll {0}", dllFile);

    using FileStream fs = new FileStream(destination, FileMode.Create);
    using ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Create);
    archive.CreateEntryFromFile(dllFile, System.IO.Path.GetFileName(dllFile), CompressionLevel.Optimal);
    ZipArchiveEntry zipArchiveEntry = archive.CreateEntry("Readme.txt");
    using Stream readmeStream = zipArchiveEntry.Open();

    byte[] content = Encoding.UTF8.GetBytes($"""
        Bouncy Hsm PKCS11 library
        
          Version: {version}
          For platform: {platform}
          Git commit: {gitCommit}
          
          Project site: https://github.com/harrison314/BouncyHsm
          License: BSD 3 Clausule
        """);

    readmeStream.Write(content);
    readmeStream.Flush();
}


string JoinPaths(params string[] parts)
{
    return System.IO.Path.Combine(parts);
}

Task("Default")
    .IsDependentOn(BuildTarget.BuildAll);

RunTarget(target);

