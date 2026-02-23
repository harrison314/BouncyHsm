using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

//NOTE: This class use UnsafeAccessor to set private fields in HashMLDsaSigner. After fix https://github.com/bcgit/bc-csharp/pull/655 refactor to use new API.
internal static class HashMLDsaSignerFactory
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "m_preHashOidEncoding")]
    private static extern ref byte[] SetDigestOid(HashMLDsaSigner signer);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "m_preHashDigest")]
    private static extern ref IDigest SetDigest(HashMLDsaSigner signer);

    public static ISigner CreatePrehash(CK_ML_DSA_PARAMETER_SET parameterSet,
        bool deterministic,
        IDigest prehashDigest)
    {
        MLDsaParameters parameters = TranslateParamaters(parameterSet);

        HashMLDsaSigner signer = new HashMLDsaSigner(parameters, deterministic);

        Prehash prehash = Prehash.ForDigest(prehashDigest);
        byte[] oid = DigestUtilities.GetObjectIdentifier(prehashDigest.AlgorithmName).GetEncoded(Asn1Encodable.Der);

        SetDigest(signer) = prehash;
        SetDigestOid(signer) = oid;

        return signer;
    }

    public static ISigner Create(CK_ML_DSA_PARAMETER_SET parameterSet,
        bool deterministic,
        IDigest digest)
    {
        MLDsaParameters parameters = TranslateParamaters(parameterSet);

        HashMLDsaSigner signer = new HashMLDsaSigner(parameters, deterministic);
        byte[] oid = DigestUtilities.GetObjectIdentifier(digest.AlgorithmName).GetEncoded(Asn1Encodable.Der);

        SetDigest(signer) = digest;
        SetDigestOid(signer) = oid;

        return signer;
    }

    private static MLDsaParameters TranslateParamaters(CK_ML_DSA_PARAMETER_SET parameterSet)
    {
        return parameterSet switch
        {
            CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_44 => MLDsaParameters.ml_dsa_44_with_sha512,
            CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_65 => MLDsaParameters.ml_dsa_65_with_sha512,
            CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_87 => MLDsaParameters.ml_dsa_87_with_sha512,
            _ => throw new InvalidProgramException($"Unsupported ML-DSA parameters type {parameterSet}."),
        };
    }
}
