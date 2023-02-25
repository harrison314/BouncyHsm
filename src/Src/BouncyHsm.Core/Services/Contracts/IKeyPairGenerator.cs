using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts;

internal interface IKeyPairGenerator
{
    void Init(IReadOnlyDictionary<CKA, IAttributeValue> publicKeyTemplate, IReadOnlyDictionary<CKA, IAttributeValue> privateKeyTemplate);

    (PublicKeyObject publicKey, PrivateKeyObject privateKey) Generate(SecureRandom secureRandom);
}
