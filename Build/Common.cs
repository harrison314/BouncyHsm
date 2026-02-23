using System;
using System.Collections.Generic;
using System.Text;

namespace Build;

internal static class BuildTarget
{
    public const string Clean = nameof(Clean);
    public const string BuildBouncyHsm = nameof(BuildBouncyHsm);
    public const string BuildBouncyHsmCli = nameof(BuildBouncyHsmCli);
    public const string BuildBouncyHsmClient = nameof(BuildBouncyHsmClient);
    public const string BuildAll = nameof(BuildAll);

    public const string BuildPkcs11LibWin32 = nameof(BuildPkcs11LibWin32);
    public const string BuildPkcs11LibX64 = nameof(BuildPkcs11LibX64);

    public const string RebuildDocumentation = nameof(RebuildDocumentation);
}
