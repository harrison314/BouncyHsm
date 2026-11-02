using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AspireExample.AppHost;

internal static class TestData
{
    public static byte[] GenerateP12()
    {
        string subject = "CN=My Test CA";

        using ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        CertificateRequest request = new CertificateRequest( new X500DistinguishedName(subject), ecdsa,HashAlgorithmName.SHA256);

        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(certificateAuthority: true, hasPathLengthConstraint: false, pathLengthConstraint: 0, critical: true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyCertSign |X509KeyUsageFlags.CrlSign, critical: true));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey,false));

        DateTimeOffset notBefore = DateTimeOffset.UtcNow.AddDays(-1);
        DateTimeOffset notAfter = notBefore.AddYears(10);

        using X509Certificate2 cert = request.CreateSelfSigned(notBefore, notAfter);

        return cert.Export(X509ContentType.Pfx, "password");
    }
}
