using BouncyHsm.Core.Services.Contracts.P11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public interface ICryptoApiObject
{
    CKO CkaClass
    {
        get;
    }

    AttributeValueResult GetValue(CKA attributeType);

    bool IsMatch(IEnumerable<KeyValuePair<CKA, IAttributeValue>> matchTemplate);

    uint? TryGetSize(bool isLoggedIn);

    void Accept(ICryptoApiObjectVisitor visitor);

    T Accept<T>(ICryptoApiObjectVisitor<T> visitor);

}
