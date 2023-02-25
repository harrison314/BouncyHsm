using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class NativeAttributeValue : IAttributeValue
{
    private readonly AttrValueFromNative value;

    public const int AttrValueFromNativeTypeByteArray = 0x01;
    public const int AttrValueFromNativeTypeBool = 0x02;
    public const int AttrValueFromNativeTypeCkUint = 0x04;
    public const int AttrValueFromNativeTypeCkDate = 0x08;


    public const int AttrValueToNativeTypeByteArray = 0x01;
    public const int AttrValueToNativeTypeBool = 0x02;
    public const int AttrValueToNativeTypeCkUint = 0x04;
    public const int AttrValueToNativeTypeCkDate = 0x08;

    public AttrTypeTag TypeTag
    {
        get;
    }

    public NativeAttributeValue(AttrValueFromNative value)
    {
        this.TypeTag = AttrTypeUtils.GetTypeTag((CKA)value.AttributeType);
        this.CheckValueType(this.TypeTag, value);
        this.value = value;
    }

    public bool AsBool()
    {
        this.CheckValueType(AttrTypeTag.CkBool);

        return this.value.ValueBool;
    }

    public byte[] AsByteArray()
    {
        this.CheckValueType(AttrTypeTag.ByteArray);

        return this.value.ValueRawBytes;
    }

    public string AsString()
    {
        this.CheckValueType(AttrTypeTag.String);

        return Encoding.UTF8.GetString(this.value.ValueRawBytes);
    }

    public uint AsUint()
    {
        this.CheckValueType(AttrTypeTag.CkUint);

        return this.value.ValueCkUlong;
    }

    public CkDate AsDate()
    {
        this.CheckValueType(AttrTypeTag.DateTime);
        return CkDate.Parse(this.value.ValueCkDate);
    }

    private void CheckValueType(AttrTypeTag tag, [CallerMemberName] string fnName = "")
    {
        if (this.TypeTag != tag)
        {
            throw new InvalidATtributeTypeCastException(AttrTypeTag.CkUint, fnName);
        }
    }

    private void CheckValueType(AttrTypeTag typeTag, AttrValueFromNative value)
    {
        if (typeTag == AttrTypeTag.CkBool && !((value.ValueTypeHint & AttrValueFromNativeTypeBool) == AttrValueFromNativeTypeBool))
        {
            throw new InvalidATtributeTypeCastException($"Attribute type {(CKA)value.AttributeType} requires type BOOL - mishmash type.");
        }

        if (typeTag == AttrTypeTag.CkUint && !((value.ValueTypeHint & AttrValueFromNativeTypeCkUint) == AttrValueFromNativeTypeCkUint))
        {
            throw new InvalidATtributeTypeCastException($"Attribute type {(CKA)value.AttributeType} requires type UINT - mishmash type.");
        }

        if (typeTag == AttrTypeTag.DateTime && !((value.ValueTypeHint & AttrValueFromNativeTypeCkDate) == AttrValueFromNativeTypeCkDate))
        {
            throw new InvalidATtributeTypeCastException($"Attribute type {(CKA)value.AttributeType} requires type CkDate - mishmash type.");
        }
    }

    public bool Equals(IAttributeValue? other)
    {
        if (other == null || this.TypeTag != other.TypeTag)
        {
            return false;
        }

        return this.TypeTag switch
        {
            AttrTypeTag.ByteArray => this.AsByteArray().SequenceEqual(other.AsByteArray()),
            AttrTypeTag.CkBool => this.AsBool() == other.AsBool(),
            AttrTypeTag.CkUint => this.AsUint() == other.AsUint(),
            AttrTypeTag.String => string.Equals(this.AsString(), other.AsString(), StringComparison.OrdinalIgnoreCase),
            AttrTypeTag.DateTime => this.AsDate().Equals(other.AsDate()),
            _ => throw new InvalidProgramException($"Enum value {this.TypeTag} is not supported.")
        };
    }

    public bool Equals(uint other)
    {
        if (this.TypeTag != AttrTypeTag.CkUint)
        {
            return false;
        }

        return this.AsUint() == other;
    }

    public uint GuessSize()
    {
        return this.TypeTag switch
        {
            AttrTypeTag.ByteArray => (uint)this.value.ValueRawBytes.Length,
            AttrTypeTag.CkBool => 1U,
            AttrTypeTag.CkUint => 4U,
            AttrTypeTag.DateTime => this.AsDate().HasValue ? 8U : 0U,
            AttrTypeTag.String => (uint)Encoding.UTF8.GetByteCount(this.AsString()),
            _ => throw new InvalidProgramException($"Enum value {this.TypeTag} is not supported.")
        };
    }

    public override string ToString()
    {
        string value = this.TypeTag switch
        {
            AttrTypeTag.ByteArray => Convert.ToBase64String(this.AsByteArray()),
            AttrTypeTag.CkBool => this.AsBool().ToString(),
            AttrTypeTag.CkUint => this.AsUint().ToString(),
            AttrTypeTag.String => $"`{this.AsString()}`",
            AttrTypeTag.DateTime=> this.AsDate().ToString(),
            _ => throw new InvalidProgramException($"Enum value {this.TypeTag} is not supported.")
        };
        return $"NativeAttributeValue: {this.TypeTag} - {value}";
    }
}
