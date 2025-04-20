using BouncyHsm.Core.Services.Contracts.Entities;
using Org.BouncyCastle.Crypto;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal interface ICipherWrapper
{
    CipherUinion IntoEncryption(KeyObject keyObject);

    IWrapper IntoWrapping(KeyObject keyObject);

    CipherUinion IntoDecryption(KeyObject keyObject);

    IWrapper IntoUnwrapping(KeyObject keyObject);
}
