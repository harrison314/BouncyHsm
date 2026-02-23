using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Tests.Services.P11Handlers.Common;

[TestClass]
public class HashSlhDsaSignerFactoryTests
{
    [TestMethod]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_128S)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_128S)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_128F)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_128F)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_192S)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_192S)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_192F)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_192F)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_256S)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_256S)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_256F)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_256F)]
    public void CreatePrehash_Call_Success(CK_SLH_DSA_PARAMETER_SET parameterSet)
    {
        ISigner signer = HashSlhDsaSignerFactory.CreatePrehash(parameterSet, true, new Sha256Digest());
        Assert.IsNotNull(signer);
    }

    [TestMethod]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_128S)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_128S)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_128F)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_128F)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_192S)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_192S)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_192F)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_192F)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_256S)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_256S)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_256F)]
    [DataRow(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_256F)]
    public void Create_Call_Success(CK_SLH_DSA_PARAMETER_SET parameterSet)
    {
        ISigner signer = HashSlhDsaSignerFactory.Create(parameterSet, true, new Sha256Digest());
        Assert.IsNotNull(signer);
    }

    [TestMethod]
    public void CreatePrehash_SignAndverify_Success()
    {
        SecureRandom secureRandom = new SecureRandom();

        byte[] dataToSign = new byte[128];
        secureRandom.NextBytes(dataToSign);

        byte[] digestValue = DigestUtilities.CalculateDigest("SHA-512", dataToSign);

        SlhDsaKeyPairGenerator slHDsaKeyPairGenerator = new SlhDsaKeyPairGenerator();
        slHDsaKeyPairGenerator.Init(new SlhDsaKeyGenerationParameters(secureRandom, SlhDsaParameters.slh_dsa_sha2_192s));
        AsymmetricCipherKeyPair keyPair = slHDsaKeyPairGenerator.GenerateKeyPair();

        ISigner signer = HashSlhDsaSignerFactory.CreatePrehash(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_192S, true, new Sha512Digest());
        signer.Init(true, keyPair.Private);
        signer.BlockUpdate(digestValue, 0, digestValue.Length);
        byte[] signature = signer.GenerateSignature();

        HashSlhDsaSigner verifier = new HashSlhDsaSigner(SlhDsaParameters.slh_dsa_sha2_192s_with_sha512, true);
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

        SlhDsaKeyPairGenerator slHDsaKeyPairGenerator = new SlhDsaKeyPairGenerator();
        slHDsaKeyPairGenerator.Init(new SlhDsaKeyGenerationParameters(secureRandom, SlhDsaParameters.slh_dsa_sha2_192s));
        AsymmetricCipherKeyPair keyPair = slHDsaKeyPairGenerator.GenerateKeyPair();

        ISigner signer = HashSlhDsaSignerFactory.Create(CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_192S, true, new Sha512Digest());
        signer.Init(true, keyPair.Private);
        signer.BlockUpdate(dataToSign, 0, dataToSign.Length);
        byte[] signature = signer.GenerateSignature();

        HashSlhDsaSigner verifier = new HashSlhDsaSigner(SlhDsaParameters.slh_dsa_sha2_192s_with_sha512, true);
        verifier.Init(false, keyPair.Public);
        verifier.BlockUpdate(dataToSign, 0, dataToSign.Length);
        bool isVerified = verifier.VerifySignature(signature);

        Assert.IsTrue(isVerified);
    }
}
