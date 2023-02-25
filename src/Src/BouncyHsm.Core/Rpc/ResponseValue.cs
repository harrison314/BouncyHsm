using System.Buffers;

namespace BouncyHsm.Core.Rpc;

public struct ResponseValue : IDisposable
{
    private readonly IMemoryOwner<byte> header;
    private readonly IMemoryOwner<byte> body;

    public ReadOnlyMemory<byte> Header
    {
        get => this.header.Memory;
    }

    public ReadOnlyMemory<byte> Body
    {
        get => this.body.Memory;
    }

    internal ResponseValue(IMemoryOwner<byte> header, IMemoryOwner<byte> body)
    {
        this.header = header;
        this.body = body;
    }

    public void Dispose()
    {
        this.body.Dispose();
        this.header.Dispose();
    }
}
