using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Aspire.Hosting;

public record BouncyHsmImage(string Image, string? Tag)
{
    private const string BaseImage = "ghcr.io/harrison314/bouncyhsm";

    public static BouncyHsmImage Current = new BouncyHsmImage(BaseImage, "v2.3.2");
    public static BouncyHsmImage V2_3_2 = new BouncyHsmImage(BaseImage, "v2.3.2");
}
