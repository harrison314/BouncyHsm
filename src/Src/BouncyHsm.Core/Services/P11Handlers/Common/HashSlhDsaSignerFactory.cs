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
internal static class HashSlhDsaSignerFactory
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "m_preHashOidEncoding")]
    private static extern ref byte[] SetDigestOid(HashSlhDsaSigner signer);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "m_preHashDigest")]
    private static extern ref IDigest SetDigest(HashSlhDsaSigner signer);

    public static ISigner CreatePrehash(CK_SLH_DSA_PARAMETER_SET parameterSet,
        bool deterministic,
        IDigest prehashDigest)
    {
        SlhDsaParameters parameters = TranslateParamaters(parameterSet);
        HashSlhDsaSigner signer = new HashSlhDsaSigner(parameters, deterministic);

        Prehash prehash = Prehash.ForDigest(prehashDigest);
        byte[] oid = DigestUtilities.GetObjectIdentifier(prehashDigest.AlgorithmName).GetEncoded(Asn1Encodable.Der);

        SetDigest(signer) = prehash;
        SetDigestOid(signer) = oid;

        return signer;
    }

    public static ISigner Create(CK_SLH_DSA_PARAMETER_SET parameterSet,
        bool deterministic,
        IDigest digest)
    {
        SlhDsaParameters parameters = TranslateParamaters(parameterSet);
        HashSlhDsaSigner signer = new HashSlhDsaSigner(parameters, deterministic);
        byte[] oid = DigestUtilities.GetObjectIdentifier(digest.AlgorithmName).GetEncoded(Asn1Encodable.Der);

        SetDigest(signer) = digest;
        SetDigestOid(signer) = oid;

        return signer;
    }

    private static SlhDsaParameters TranslateParamaters(CK_SLH_DSA_PARAMETER_SET parameterSet)
    {
        return parameterSet switch
        {
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_128S => SlhDsaParameters.slh_dsa_sha2_128s_with_sha256,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_128S => SlhDsaParameters.slh_dsa_shake_128s_with_shake128,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_128F => SlhDsaParameters.slh_dsa_sha2_128f_with_sha256,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_128F => SlhDsaParameters.slh_dsa_shake_128f_with_shake128,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_192S => SlhDsaParameters.slh_dsa_sha2_192s_with_sha512,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_192S => SlhDsaParameters.slh_dsa_shake_192s_with_shake256,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_192F => SlhDsaParameters.slh_dsa_sha2_192f_with_sha512,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_192F => SlhDsaParameters.slh_dsa_shake_192f_with_shake256,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_256S => SlhDsaParameters.slh_dsa_sha2_256s_with_sha512,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_256S => SlhDsaParameters.slh_dsa_shake_256s_with_shake256,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_256F => SlhDsaParameters.slh_dsa_sha2_256f_with_sha512,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_256F => SlhDsaParameters.slh_dsa_shake_256f_with_shake256,
            _ => throw new InvalidProgramException($"Enum value {parameterSet} is not supported.")
        };
    }
}
