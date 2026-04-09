using BouncyHsm.Core.Services.Contracts.P11;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Core.Services.Contracts.Entities.Attributes;

internal class UintArrayAttributeValue : IAttributeValue
{
    private readonly uint[] value;

    public AttrTypeTag TypeTag
    {
        get => AttrTypeTag.UintArray;
    }

    public UintArrayAttributeValue(uint[] value)
    {
        this.value = value;
    }

    public bool AsBool()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.UintArray);
    }

    public byte[] AsByteArray()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.UintArray);
    }

    public CkDate AsDate()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.UintArray);
    }

    public string AsString()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.UintArray);
    }

    public uint AsUint()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.UintArray);
    }

    public uint[] AsUintArray()
    {
        return this.value;
    }

    public bool Equals(IAttributeValue? other)
    {
        if (other == null || other.TypeTag != AttrTypeTag.UintArray)
        {
            return false;
        }

        return this.value.SequenceEqual(other.AsUintArray());
    }

    public bool Equals(uint other)
    {
        return false;
    }

    public uint GuessSize()
    {
        return 4 * (uint)this.value.Length;
    }
}
