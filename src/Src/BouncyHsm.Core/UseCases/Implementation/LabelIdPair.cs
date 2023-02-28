using BouncyHsm.Core.Services.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.UseCases.Implementation;

internal struct LabelIdPair : IEquatable<LabelIdPair>
{
    private readonly int hashCode;

    public string CkaLabel
    {
        get;
    }

    public byte[] CkaId
    {
        get;
    }

    public LabelIdPair(string ckaLabel, byte[] ckaId)
    {
        this.CkaLabel = ckaLabel;
        this.CkaId = ckaId;

        this.hashCode = ckaLabel.GetHashCode() ^ GetHashCodeInternal(ckaId);
    }

    public bool Equals(LabelIdPair other)
    {
        if (this.hashCode != other.hashCode)
        {
            return false;
        }

        return string.Equals(this.CkaLabel, other.CkaLabel, StringComparison.Ordinal) && this.CkaId.SequenceEqual(other.CkaId);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is LabelIdPair other)
        {
            return this.Equals(other: other);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return this.hashCode;
    }

    public override string ToString()
    {
        return $"LabelIdPair {{ CkaLabel: {this.CkaLabel}, CkaId: {HexConvertor.GetString(this.CkaId)} }}";
    }

    private static int GetHashCodeInternal(ReadOnlySpan<byte> data)
    {
        int i = data.Length;
        int hc = i + 1;

        while (--i >= 0)
        {
            hc *= 257;
            hc ^= data[i];
        }

        return hc;
    }

    private static int GethashCodeInternal(ReadOnlySpan<char> data)
    {
        int i = data.Length;
        int hc = i + 1;

        while (--i >= 0)
        {
            hc *= 257;
            hc ^= (int)data[i];
        }

        return hc;
    }
}
