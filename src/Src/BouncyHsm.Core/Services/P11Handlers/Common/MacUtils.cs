using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal static class MacUtils
{
    public static IMac? TryGetPrf(CKM prf)
    {
        return prf switch
        {
            CKM.CKM_SHA_1_HMAC => new HMac(new Sha1Digest()),
            CKM.CKM_SHA224_HMAC => new HMac(new Sha224Digest()),
            CKM.CKM_SHA256_HMAC => new HMac(new Sha256Digest()),
            CKM.CKM_SHA384_HMAC => new HMac(new Sha384Digest()),
            CKM.CKM_SHA512_HMAC => new HMac(new Sha512Digest()),
            CKM.CKM_SHA3_224_HMAC => new HMac(new Sha3Digest(224)),
            CKM.CKM_SHA3_256_HMAC => new HMac(new Sha3Digest(256)),
            CKM.CKM_SHA3_384_HMAC => new HMac(new Sha3Digest(384)),
            CKM.CKM_SHA3_512_HMAC => new HMac(new Sha3Digest(512)),
            CKM.CKM_DES3_CMAC => throw new NotSupportedException("Algorithm CKM_DES3_CMAC is not supported."),
            CKM.CKM_AES_CMAC => new CMac(AesUtilities.CreateEngine()),
            _ => null
        };
    }
}
