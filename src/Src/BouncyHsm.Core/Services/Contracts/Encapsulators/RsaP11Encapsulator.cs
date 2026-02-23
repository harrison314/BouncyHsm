using BouncyHsm.Core.Services.Bc;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.Security.Cryptography.X509Certificates;

namespace BouncyHsm.Core.Services.Contracts.Encapsulators;

internal class RsaP11Encapsulator : P11EncapsulatorBase<RsaPublicKeyObject, RsaPrivateKeyObject>
{
    private readonly IBufferedCipher bufferedCipher;

    public RsaP11Encapsulator(IBufferedCipher bufferedCipher,
        ILogger<RsaP11Encapsulator> logger,
        CKM mechynismType) : base(logger, mechynismType)
    {
        this.bufferedCipher = bufferedCipher;
    }

    protected override void EncapsulateInternal(RsaPublicKeyObject publicKey, SecretKeyObject secretKeyObject, SecureRandom secureRandom, out byte[] encapsulatedData)
    {
        int minSize = this.GetMinimalSecretLength();
        byte[] secret = new byte[minSize];
        secureRandom.NextBytes(secret);

        BufferedCipherWrapper wrapper = new BufferedCipherWrapper(this.bufferedCipher, false);
        wrapper.Init(true, publicKey.GetPublicKey());

        encapsulatedData = wrapper.Wrap(secret, 0, secret.Length);
        secretKeyObject.SetSecret(secret);
    }

    protected override void DecapsulateInternal(RsaPrivateKeyObject privateKey, byte[] encapsulatedData, SecretKeyObject secretKeyObject)
    {
        BufferedCipherWrapper wrapper = new BufferedCipherWrapper(this.bufferedCipher, false);
        wrapper.Init(false, privateKey.GetPrivateKey());

        byte[] secret = wrapper.Unwrap(encapsulatedData, 0, encapsulatedData.Length);

        this.SetSecretKeyPadded(secretKeyObject, secret);
    }

    protected override int GetEncapsulatedDataLengthInternal(RsaPublicKeyObject publicKey)
    {
        this.bufferedCipher.Init(true, publicKey.GetPublicKey());
        return this.bufferedCipher.GetOutputSize(this.GetMinimalSecretLength());
    }

    public override string ToString()
    {
        return $"RsaP11Encapsulator with {this.bufferedCipher.AlgorithmName}";
    }
}
