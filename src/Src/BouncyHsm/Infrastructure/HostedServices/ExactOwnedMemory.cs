using System.Buffers;
using System.Runtime.CompilerServices;

namespace BouncyHsm.Infrastructure.HostedServices;

internal struct ExactOwnedMemory : IDisposable
{
    private readonly byte[] bytes;

    public Memory<byte> Memory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    private ExactOwnedMemory(byte[] bytes, int length)
    {
        this.Memory = bytes.AsMemory(0, length);
        this.bytes = bytes;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ExactOwnedMemory Rent(int length)
    {
        System.Diagnostics.Debug.Assert(length > 0);

        return new ExactOwnedMemory(ArrayPool<byte>.Shared.Rent(length), length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(this.bytes, false);
    }
}