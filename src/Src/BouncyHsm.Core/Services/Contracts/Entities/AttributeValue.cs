using BouncyHsm.Core.Services.Contracts.Entities.Attributes;
using BouncyHsm.Core.Services.Contracts.P11;
using System.Runtime.CompilerServices;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public static class AttributeValue
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAttributeValue Create(bool value)
    {
        return value ? BoolAttributeValue.True : BoolAttributeValue.False;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAttributeValue Create(uint value)
    {
        return new UintAttributeValue(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAttributeValue Create(string value)
    {
        return new StringAttributeValue(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAttributeValue Create(byte[] value)
    {
        return (value.Length == 0) ? ByteArrayAttributeValue.Empty : new ByteArrayAttributeValue(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAttributeValue Create(CkDate date)
    {
        return (date.HasValue) ? new CkDateAttributeValue(date) : CkDateAttributeValue.Empty;
    }
}
