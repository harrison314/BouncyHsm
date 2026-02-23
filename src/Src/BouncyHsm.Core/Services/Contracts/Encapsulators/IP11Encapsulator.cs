using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.Encapsulators;

internal interface IP11Encapsulator
{
    void Init(Dictionary<CKA, IAttributeValue> template);

    EncapsulationResult Encapsulate(PublicKeyObject publicKey, SecureRandom secureRandom);

    uint GetEncapsulatedDataLength(PublicKeyObject publicKey);

    SecretKeyObject Decapsulate(PrivateKeyObject privateKey, byte[] encapsulatedData);
}
