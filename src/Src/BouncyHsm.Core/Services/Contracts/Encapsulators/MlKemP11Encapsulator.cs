using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Kems;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.Encapsulators;

internal class MlKemP11Encapsulator : P11EncapsulatorBase<MlKemPublicKeyObject, MlKemPrivateKeyObject>
{
    public MlKemP11Encapsulator(ILogger<MlKemP11Encapsulator> logger)
        : base(logger, CKM.CKM_ML_KEM)
    {
    }

    protected override void EncapsulateInternal(MlKemPublicKeyObject publicKey, SecretKeyObject secretKeyObject, SecureRandom secureRandom, out byte[] encapsulatedData)
    {
        this.logger.LogTrace("Entering to EncapsulateInternal for ML-KEM using parameter set {CkaParameterSet}.", publicKey.CkaParameterSet);

        MLKemEncapsulator encapsulator = new MLKemEncapsulator(MlKemUtils.GetParametersFromType(publicKey.CkaParameterSet));
        encapsulator.Init(new ParametersWithRandom(publicKey.GetPublicKey(), secureRandom));

        byte[] encapsulation = new byte[encapsulator.EncapsulationLength];
        byte[] secret = new byte[encapsulator.SecretLength];

        encapsulator.Encapsulate(encapsulation.AsSpan(), secret.AsSpan());

        this.SetSecretKeyPadded(secretKeyObject, secret);
        encapsulatedData = encapsulation;
    }

    protected override int GetEncapsulatedDataLengthInternal(MlKemPublicKeyObject publicKey)
    {
        MLKemEncapsulator encapsulator = new MLKemEncapsulator(MlKemUtils.GetParametersFromType(publicKey.CkaParameterSet));
        encapsulator.Init(publicKey.GetPublicKey());

        return encapsulator.EncapsulationLength;
    }

    protected override void DecapsulateInternal(MlKemPrivateKeyObject privateKey, byte[] encapsulatedData, SecretKeyObject secretKeyObject)
    {
        this.logger.LogTrace("Entering to DecapsulateInternal for ML-KEM using parameter set {CkaParameterSet}.", privateKey.CkaParameterSet);

        MLKemDecapsulator decapsulator = new MLKemDecapsulator(MlKemUtils.GetParametersFromType(privateKey.CkaParameterSet));
        decapsulator.Init(privateKey.GetPrivateKey());

        byte[] secret = new byte[decapsulator.SecretLength];
        decapsulator.Decapsulate(encapsulatedData.AsSpan(), secret.AsSpan());

        this.SetSecretKeyPadded(secretKeyObject, secret);
    }

    public override string ToString()
    {
        return "MlKemP11Encapsulator";
    }
}
