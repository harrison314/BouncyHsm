using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Tests.Services.P11Handlers.Common;

[TestClass]
public class HashMLDsaSignerFactoryTests
{
    [TestMethod]
    [DataRow(CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_44)]
    [DataRow(CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_65)]
    [DataRow(CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_87)]
    public void CreatePrehash_Call_Success(CK_ML_DSA_PARAMETER_SET parameterSet)
    {
        ISigner signer = HashMLDsaSignerFactory.CreatePrehash(parameterSet, true, new Sha256Digest());
        Assert.IsNotNull(signer);
    }

    [TestMethod]
    [DataRow(CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_44)]
    [DataRow(CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_65)]
    [DataRow(CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_87)]
    public void Create_Call_Success(CK_ML_DSA_PARAMETER_SET parameterSet)
    {
        ISigner signer = HashMLDsaSignerFactory.Create(parameterSet, true, new Sha256Digest());
        Assert.IsNotNull(signer);
    }

    [TestMethod]
    public void CreatePrehash_SignAndverify_Success()
    {
        SecureRandom secureRandom = new SecureRandom();

        byte[] dataToSign = new byte[128];
        secureRandom.NextBytes(dataToSign);

        byte[] digestValue = DigestUtilities.CalculateDigest("SHA-512", dataToSign);

        MLDsaKeyPairGenerator mLDsaKeyPairGenerator = new MLDsaKeyPairGenerator();
        mLDsaKeyPairGenerator.Init(new MLDsaKeyGenerationParameters(secureRandom, MLDsaParameters.ml_dsa_65));
        AsymmetricCipherKeyPair keyPair = mLDsaKeyPairGenerator.GenerateKeyPair();

        ISigner signer = HashMLDsaSignerFactory.CreatePrehash(CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_65, true, new Sha512Digest());
        signer.Init(true, keyPair.Private);
        signer.BlockUpdate(digestValue, 0, digestValue.Length);
        byte[] signature = signer.GenerateSignature();

        HashMLDsaSigner verifier = new HashMLDsaSigner(MLDsaParameters.ml_dsa_65_with_sha512, true);
        verifier.Init(false, keyPair.Public);
        verifier.BlockUpdate(dataToSign, 0, dataToSign.Length);
        bool isVerified = verifier.VerifySignature(signature);

        Assert.IsTrue(isVerified);
    }

    [TestMethod]
    public void Create_SignAndverify_Success()
    {
        SecureRandom secureRandom = new SecureRandom();

        byte[] dataToSign = new byte[128];
        secureRandom.NextBytes(dataToSign);

        MLDsaKeyPairGenerator mLDsaKeyPairGenerator = new MLDsaKeyPairGenerator();
        mLDsaKeyPairGenerator.Init(new MLDsaKeyGenerationParameters(secureRandom, MLDsaParameters.ml_dsa_65));
        AsymmetricCipherKeyPair keyPair = mLDsaKeyPairGenerator.GenerateKeyPair();

        ISigner signer = HashMLDsaSignerFactory.Create(CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_65, true, new Sha512Digest());
        signer.Init(true, keyPair.Private);
        signer.BlockUpdate(dataToSign, 0, dataToSign.Length);
        byte[] signature = signer.GenerateSignature();

        HashMLDsaSigner verifier = new HashMLDsaSigner(MLDsaParameters.ml_dsa_65_with_sha512, true);
        verifier.Init(false, keyPair.Public);
        verifier.BlockUpdate(dataToSign, 0, dataToSign.Length);
        bool isVerified = verifier.VerifySignature(signature);

        Assert.IsTrue(isVerified);
    }
}
