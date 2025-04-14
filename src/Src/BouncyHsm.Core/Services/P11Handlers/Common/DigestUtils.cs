using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal static class DigestUtils
{
    public static IDigest? TryGetDigest(CKM mechanism)
    {
        return mechanism switch
        {
            CKM.CKM_SHA_1 => new Sha1Digest(),
            CKM.CKM_SHA224 => new Sha224Digest(),
            CKM.CKM_SHA256 => new Sha256Digest(),
            CKM.CKM_SHA384 => new Sha384Digest(),
            CKM.CKM_SHA512 => new Sha512Digest(),
            CKM.CKM_SHA512_256 => new Sha512tDigest(256),
            CKM.CKM_SHA512_224 => new Sha512tDigest(224),
            CKM.CKM_RIPEMD128 => new RipeMD128Digest(),
            CKM.CKM_RIPEMD160 => new RipeMD160Digest(),
            CKM.CKM_GOSTR3411 => new Gost3411Digest(),
            CKM.CKM_SHA3_256 => new Sha3Digest(256),
            CKM.CKM_SHA3_224 => new Sha3Digest(224),
            CKM.CKM_SHA3_384 => new Sha3Digest(384),
            CKM.CKM_SHA3_512 => new Sha3Digest(512),
            CKM.CKM_BLAKE2B_160 => new Blake2bDigest(160),
            CKM.CKM_BLAKE2B_256 => new Blake2bDigest(256),
            CKM.CKM_BLAKE2B_384 => new Blake2bDigest(384),
            CKM.CKM_BLAKE2B_512 => new Blake2bDigest(512),

            _ => null
        };
    }

    public static byte[] Compute(CKM mechanism, ReadOnlySpan<byte> data)
    {
        IDigest? digest = TryGetDigest(mechanism);
        if (digest == null) throw new ArgumentException("mechanism is not digest mechanism.");

        digest.BlockUpdate(data);

        byte[] hash = new byte[digest.GetDigestSize()];
        digest.DoFinal(hash.AsSpan());

        return hash;
    }

    public static byte[] ComputeCheckValue(ReadOnlySpan<byte> data)
    {
        Sha1Digest sha1Digest = new Sha1Digest();
        Span<byte> buffer = stackalloc byte[sha1Digest.GetDigestSize()];
        sha1Digest.BlockUpdate(data);
        sha1Digest.DoFinal(buffer);

        byte[] checkValue = new byte[3];
        buffer.Slice(0, 3).CopyTo(checkValue);

        return checkValue;
    }

    public static IDigest? TryGetDigest(CKG mgf)
    {
        return mgf switch
        {
            CKG.CKG_MGF1_SHA1 => new Sha1Digest(),
            CKG.CKG_MGF1_SHA224 => new Sha224Digest(),
            CKG.CKG_MGF1_SHA256 => new Sha256Digest(),
            CKG.CKG_MGF1_SHA384 => new Sha384Digest(),
            CKG.CKG_MGF1_SHA512 => new Sha512Digest(),
            CKG.CKG_MGF1_SHA3_256 => new Sha3Digest(256),
            CKG.CKG_MGF1_SHA3_224 => new Sha3Digest(224),
            CKG.CKG_MGF1_SHA3_384 => new Sha3Digest(384),
            CKG.CKG_MGF1_SHA3_512 => new Sha3Digest(512),
            _ => null
        };
    }
}
