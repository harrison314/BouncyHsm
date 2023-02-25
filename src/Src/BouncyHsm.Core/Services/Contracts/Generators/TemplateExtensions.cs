using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal static class TemplateExtensions
{
    public static uint GetRequiredAttributeUint(this IReadOnlyDictionary<CKA, IAttributeValue> template, CKA attributeType)
    {
        if (template.TryGetValue(attributeType, out IAttributeValue? value))
        {
            if (value.TypeTag != AttrTypeTag.CkUint)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, $"Attribute {attributeType} must by CK_UINT.");
            }

            return value.AsUint();
        }

        throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCOMPLETE, $"Missing attribute {attributeType}.");
    }

    public static byte[] GetRequiredAttributeBytes(this IReadOnlyDictionary<CKA, IAttributeValue> template, CKA attributeType)
    {
        if (template.TryGetValue(attributeType, out IAttributeValue? value))
        {
            if (value.TypeTag != AttrTypeTag.ByteArray)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, $"Attribute {attributeType} must by byte array.");
            }

            return value.AsByteArray();
        }

        throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCOMPLETE, $"Missing attribute {attributeType}.");
    }

    public static byte[] GetAttributeBytes(this IReadOnlyDictionary<CKA, IAttributeValue> template, CKA attributeType, byte[] defaultValue)
    {
        if (template.TryGetValue(attributeType, out IAttributeValue? value))
        {
            if (value.TypeTag != AttrTypeTag.ByteArray)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, $"Attribute {attributeType} must by byte array.");
            }

            return value.AsByteArray();
        }

        return defaultValue;
    }

    public static uint GetAttributeUint(this IReadOnlyDictionary<CKA, IAttributeValue> template, CKA attributeType, uint defaultValue)
    {
        if (template.TryGetValue(attributeType, out IAttributeValue? value))
        {
            if (value.TypeTag != AttrTypeTag.CkUint)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, $"Attribute {attributeType} must by CK_UINT.");
            }

            return value.AsUint();
        }

        return defaultValue;
    }
}
