using BouncyHsm.Core.Services.Utils;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.X509;

namespace BouncyHsm.Core.Services.Bc;

public static class X509CertificateExtensions
{
    public static string GetThumbprint(this X509Certificate certificate)
    {
        Sha1Digest sha1 = new Sha1Digest();
        sha1.BlockUpdate(certificate.GetEncoded());

        Span<byte> thumbprint = stackalloc byte[20];
        sha1.DoFinal(thumbprint);

        return HexConvertor.GetString(thumbprint);
    }
}