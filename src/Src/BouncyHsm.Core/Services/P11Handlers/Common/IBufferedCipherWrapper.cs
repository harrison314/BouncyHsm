using BouncyHsm.Core.Services.Contracts.Entities;
using Org.BouncyCastle.Crypto;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal interface IBufferedCipherWrapper
{
    IBufferedCipher IntoEncryption(KeyObject keyObject);

    IWrapper IntoWraping(KeyObject keyObject);

    IBufferedCipher IntoDecryption(KeyObject keyObject);

    IWrapper IntoUnwraping(KeyObject keyObject);
}