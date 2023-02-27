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
            AbsolutePath detinationDoc = RootDirectory / "Doc" / "SuportedAlgorithms.md";
            
            DotNet($"run -c {Configuration} -p:GitCommit={Repository.Commit} -- \"{detinationDoc}\"",
                workingDirectory: RootDirectory / "src" / "Tools" / "BouncyHsm.DocGenerator");
        });
}