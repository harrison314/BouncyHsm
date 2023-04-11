using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Infrastructure.Storage.InMemory;

internal static class StorageObjectMementoExtensions
{
    public static bool GetCkaPrivate(this StorageObjectMemento storageObjectMemento)
    {
        return storageObjectMemento.Values[CKA.CKA_PRIVATE].AsBool();
    }

    public static bool IsMatch(this StorageObjectMemento storageObjectMemento, IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        StorageObject storageObject = StorageObjectFactory.CreateFromMemento(storageObjectMemento);
        return storageObject.IsMatch(template);
    }

    public static bool IsPrivateKey(this StorageObjectMemento storageObjectMemento)
    {
        return storageObjectMemento.Values[CKA.CKA_CLASS].Equals((uint)CKO.CKO_PRIVATE_KEY);
    }

    public static bool IsX509Certificate(this StorageObjectMemento storageObjectMemento)
    {
        return storageObjectMemento.Values[CKA.CKA_CLASS].Equals((uint)CKO.CKO_CERTIFICATE)
            && storageObjectMemento.Values[CKA.CKA_CERTIFICATE_TYPE].Equals((uint)CKC.CKC_X_509);
    }
}
