using System;
using System.Collections.Generic;
using System.Text;

namespace Aspire.Hosting;

public record BouncyHsmImage(string Image, string? Tag)
{
    public static BouncyHsmImage Current = new BouncyHsmImage("ghcr.io/harrison314/bouncyhsm", "v2.3.2");
    public static BouncyHsmImage V2_3_2 = new BouncyHsmImage("ghcr.io/harrison314/bouncyhsm", "v2.3.2");
}
