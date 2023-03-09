using Microsoft.JSInterop;

namespace BouncyHsm.Spa.Shared;

internal static class JSRuntimeExtensions
{
    public static async Task DownloadFile(this IJSRuntime js, string fileName, byte[] content)
    {
        using MemoryStream ms = new MemoryStream(content);
        using DotNetStreamReference streamRef = new DotNetStreamReference(stream: ms);

        await js.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }
}
