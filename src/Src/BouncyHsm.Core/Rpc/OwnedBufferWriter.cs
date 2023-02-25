using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Rpc;

internal sealed class OwnedBufferWriter : IBufferWriter<byte>, IMemoryOwner<byte>
{
    private byte[] array;
    private int index;

    public Memory<byte> Memory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.array.AsMemory(0, this.index);
    }

    public OwnedBufferWriter(int initialSize)
    {
        System.Diagnostics.Debug.Assert(initialSize > 0);

        this.index = 0;
        this.array = ArrayPool<byte>.Shared.Rent(initialSize);
    }

    public void Advance(int count)
    {
        byte[] buffer = this.array;

        if (this.index > buffer.Length - count)
        {
            ThrowArgumentExceptionForAdvancedTooFar();
        }

        this.index += count;
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        CheckBufferAndEnsureCapacity(sizeHint);

        return this.array.AsMemory(this.index);
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        CheckBufferAndEnsureCapacity(sizeHint);

        return this.array.AsSpan(this.index);
    }

    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(this.array);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckBufferAndEnsureCapacity(int sizeHint)
    {
        byte[] array = this.array;

        System.Diagnostics.Debug.Assert(sizeHint >= 0);

        if (sizeHint == 0)
        {
            sizeHint = 1;
        }

        if (sizeHint > array!.Length - this.index)
        {
            this.ResizeBuffer(sizeHint);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ResizeBuffer(int sizeHint)
    {
        byte[] currentBuffer = this.array;

        uint minimumSize = (uint)this.index + (uint)sizeHint;
        uint doubleSize = 2 * (uint)currentBuffer.Length;
        if (minimumSize < doubleSize)
        {
            minimumSize = doubleSize;
        }

        if (minimumSize > 1024 * 1024)
        {
            minimumSize = BitOperations.RoundUpToPowerOf2(minimumSize);
        }
        
        byte[] newBuffer = ArrayPool<byte>.Shared.Rent((int)minimumSize);
        Buffer.BlockCopy(currentBuffer, 0, newBuffer, 0, this.index);
        
        this.array = newBuffer;

        ArrayPool<byte>.Shared.Return(currentBuffer);
    }

    private static void ThrowArgumentExceptionForAdvancedTooFar()
    {
        throw new ArgumentException("The buffer writer has advanced too far.");
    }
}
