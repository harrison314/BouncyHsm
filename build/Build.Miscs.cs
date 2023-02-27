using Nuke.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nuke.Common.IO;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Nuke.Common.Tools.DotNet;

public partial class Build
{
    Target RebuildDocumentation => _ => _
        .Executes(() =>
        {
            //AbsolutePath projectFile =  RootDirectory / "src" / "Tools" / "BouncyHsm.DocGenerator" / "BouncyHsm.DocGenerator.csproj";
            AbsolutePath detinationDoc = RootDirectory / "Doc" / "SuportedAlgorithms.md";

            DotNet($"run -c {Configuration} -- \"{detinationDoc}\"",
                workingDirectory: RootDirectory / "src" / "Tools" / "BouncyHsm.DocGenerator");
        });
}