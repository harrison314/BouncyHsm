using Microsoft.Build.Tasks;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.MSBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using System.IO.Compression;
using System.IO;

public partial class Build
{
    Target BuildPkcs11LibWin32 => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            BuildBouncyHsmPkcs11Lib(MSBuildTargetPlatform.Win32);
            AbsolutePath nativeLib = SourceDirectory / "BouncyHsm.Pkcs11Lib" / Configuration / "BouncyHsm.Pkcs11Lib.dll";
            AbsolutePath destination = ArtifactsTmpDirectory / "native" / "Win-x86";
            destination.CreateOrCleanDirectory();
            nativeLib.CopyToDirectory(destination);
        });

    Target BuildPkcs11LibX64 => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            BuildBouncyHsmPkcs11Lib(MSBuildTargetPlatform.x64);
            AbsolutePath nativeLib = SourceDirectory / "BouncyHsm.Pkcs11Lib" / "x64" / Configuration / "BouncyHsm.Pkcs11Lib.dll";
            AbsolutePath destination = ArtifactsTmpDirectory / "native" / "Win-x64";
            destination.CreateOrCleanDirectory();
            nativeLib.CopyToDirectory(destination);
        });

    private void BuildBouncyHsmPkcs11Lib(MSBuildTargetPlatform platform)
    {
        AbsolutePath p11Proj = SourceDirectory / "BouncyHsm.Pkcs11Lib" / "BouncyHsm.Pkcs11Lib.vcxproj";
        MSBuild(_ => _
           .SetTargetPath(p11Proj)
           .SetConfiguration(Configuration)
           .SetTargetPlatform(platform)
           .SetTargets("clean", "build"));
    }
}
