using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Infrastructure.Storage.LiteDbFile;

internal static class FindObjectSpecificationExtensions
{
    public static uint? TryGetUintValue(this FindObjectSpecification specification, CKA attributeType)
    {
        if (specification.Template.TryGetValue(attributeType, out Core.Services.Contracts.Entities.IAttributeValue? attributeValue))
        {
            return attributeValue.AsUint();
        }

        return null;
    }

    public static byte[]? TryGetBytesValue(this FindObjectSpecification specification, CKA attributeType)
    {
        if (specification.Template.TryGetValue(attributeType, out Core.Services.Contracts.Entities.IAttributeValue? attributeValue))
        {
            return attributeValue.AsByteArray();
        }

        return null;
    }

    public static string? TryGetStringValue(this FindObjectSpecification specification, CKA attributeType)
    {
        if (specification.Template.TryGetValue(attributeType, out Core.Services.Contracts.Entities.IAttributeValue? attributeValue))
        {
            return attributeValue.AsString();
        }

        return null;
    }
}
