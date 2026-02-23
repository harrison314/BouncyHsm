using BouncyHsm.Core.Services.Bc;
using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal static class AgreementUtils
{
    public static IBasicAgreement CreateAgreement(IBasicAgreement basicAgreement, CKD kdfFunction, int minKeySize, byte[]? sharedData)
    {
        return kdfFunction switch
        {
            CKD.CKD_NULL => basicAgreement,
            CKD.CKD_SHA1_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Sha1Digest(), sharedData),
            CKD.CKD_SHA224_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Sha224Digest(), sharedData),
            CKD.CKD_SHA256_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Sha256Digest(), sharedData),
            CKD.CKD_SHA384_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Sha384Digest(), sharedData),
            CKD.CKD_SHA512_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Sha512Digest(), sharedData),
            CKD.CKD_SHA3_224_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Sha3Digest(224), sharedData),
            CKD.CKD_SHA3_256_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Sha3Digest(256), sharedData),
            CKD.CKD_SHA3_384_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Sha3Digest(384), sharedData),
            CKD.CKD_SHA3_512_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Sha3Digest(512), sharedData),
            CKD.CKD_BLAKE2B_160_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Blake2bDigest(160), sharedData),
            CKD.CKD_BLAKE2B_256_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Blake2bDigest(256), sharedData),
            CKD.CKD_BLAKE2B_384_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Blake2bDigest(384), sharedData),
            CKD.CKD_BLAKE2B_512_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Blake2bDigest(512), sharedData),

            _ => throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"kdf {kdfFunction} from CK_ECDH1_DERIVE_PARAMS is not supported or invalid.")
        };
    }

    public static IRawAgreement CreateAgreement(IRawAgreement basicAgreement, CKD kdfFunction, byte[]? sharedData)
    {
        return kdfFunction switch
        {
            CKD.CKD_NULL => new SafeRawAgreement(basicAgreement),
            CKD.CKD_SHA1_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Sha1Digest(), sharedData),
            CKD.CKD_SHA224_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Sha224Digest(), sharedData),
            CKD.CKD_SHA256_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Sha256Digest(), sharedData),
            CKD.CKD_SHA384_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Sha384Digest(), sharedData),
            CKD.CKD_SHA512_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Sha512Digest(), sharedData),
            CKD.CKD_SHA3_224_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Sha3Digest(224), sharedData),
            CKD.CKD_SHA3_256_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Sha3Digest(256), sharedData),
            CKD.CKD_SHA3_384_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Sha3Digest(384), sharedData),
            CKD.CKD_SHA3_512_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Sha3Digest(512), sharedData),
            CKD.CKD_BLAKE2B_160_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Blake2bDigest(160), sharedData),
            CKD.CKD_BLAKE2B_256_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Blake2bDigest(256), sharedData),
            CKD.CKD_BLAKE2B_384_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Blake2bDigest(384), sharedData),
            CKD.CKD_BLAKE2B_512_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Blake2bDigest(512), sharedData),

            _ => throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"kdf {kdfFunction} from CK_ECDH1_DERIVE_PARAMS is not supported or invalid.")
        };
    }
}
