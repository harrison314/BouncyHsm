using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BouncyHsm.Core.Services.Utils;

internal ref struct BitSpan
{
    private readonly Span<byte> span;

    public ReadOnlySpan<byte> Span
    {
        get => this.span;
    }

    public int BitLength
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.span.Length * 8;
    }

    public bool this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            System.Diagnostics.Debug.Assert(index >= 0 && index < this.BitLength);

            int bitIndex = 7 - index % 8;
            return (this.span[index / 8] & (1 << bitIndex)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            System.Diagnostics.Debug.Assert(index >= 0 && index < this.BitLength);

            int bitIndex = 7 - index % 8;
            if (value)
            {
                this.span[index / 8] |= (byte)(1 << bitIndex);
            }
            else
            {
                this.span[index / 8] &= (byte)~(1 << bitIndex);
            }
        }
    }

    public BitSpan(Span<byte> span)
    {
        this.span = span;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Fill(bool value)
    {
        this.span.Fill(value ? (byte)0xFF : (byte)0x00);
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder(this.BitLength);
        for (int i = 0; i < this.BitLength; i++)
        {
            sb.Append(this[i] ? '1' : '0');
        }

        return sb.ToString();
    }
}
