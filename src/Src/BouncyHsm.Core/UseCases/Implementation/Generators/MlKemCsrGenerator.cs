using BouncyHsm.Core.Services.Bc;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Core.UseCases.Implementation.Generators;

// RFC 9883 - https://datatracker.ietf.org/doc/html/rfc9883
internal class MlKemCsrGenerator
{
    public static Pkcs10CertificationRequest GenerateRequest(TimeProvider timeProvider, X509Name subject, TimeSpan validity, MLKemPublicKeyParameters publicKey, SecureRandom secureRandom)
    {
        AsymmetricCipherKeyPair signatureKeyPair = GenerateNewMlDsaKeyPair(publicKey, secureRandom);
        X509Certificate certificate = CreateSelfSigned(subject, signatureKeyPair, timeProvider, validity);

        PrivateKeyPossessionStatement privateKeyPossessionStatement = new PrivateKeyPossessionStatement(certificate, true);

        AttributePkcs attributePkcs = new AttributePkcs(PrivateKeyPossessionStatement.id_at_statementOfPossession,
            new DerSet(privateKeyPossessionStatement));

        MLDsaPrivateKeyParameters privateKey = (MLDsaPrivateKeyParameters)signatureKeyPair.Private;

        Pkcs10CertificationRequest certificationRequest = new Pkcs10CertificationRequest(privateKey.Parameters.ParameterSet.Name,
            subject,
            publicKey,
            new DerSet(attributePkcs),
            signatureKeyPair.Private);

        return certificationRequest;
    }

    private static X509Certificate CreateSelfSigned(X509Name subject, AsymmetricCipherKeyPair signatureKeyPair, TimeProvider timeProvider, TimeSpan validity)
    {
        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        MLDsaPrivateKeyParameters privateKey = (MLDsaPrivateKeyParameters)signatureKeyPair.Private;

        Asn1SignatureFactory asn1SignatureFactory = new Asn1SignatureFactory(privateKey.Parameters.ParameterSet.Name,
           privateKey);

        X509V3CertificateGenerator generator = new X509V3CertificateGenerator();
        generator.SetIssuerDN(subject);
        generator.SetSubjectDN(subject);
        generator.SetSerialNumber(Org.BouncyCastle.Math.BigInteger.One);
        generator.SetSubjectPublicKeyInfo(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(signatureKeyPair.Public));
        generator.SetNotBefore(now);
        generator.SetNotAfter(now.Add(validity));
        generator.AddExtension(X509Extensions.KeyUsage, false, new KeyUsage(KeyUsage.DigitalSignature));
        X509Certificate certificate = generator.Generate(asn1SignatureFactory);

        return certificate;
    }

    private static AsymmetricCipherKeyPair GenerateNewMlDsaKeyPair(MLKemPublicKeyParameters publicKey, SecureRandom secureRandom)
    {
        MLDsaParameters mLDsaParameters;
        if (publicKey.Parameters.ParameterSet.Name == MLKemParameters.ml_kem_512.Name)
        {
            mLDsaParameters = MLDsaParameters.ml_dsa_44;
        }
        else if (publicKey.Parameters.ParameterSet.Name == MLKemParameters.ml_kem_768.Name)
        {
            mLDsaParameters = MLDsaParameters.ml_dsa_65;
        }
        else if (publicKey.Parameters.ParameterSet.Name == MLKemParameters.ml_kem_1024.Name)
        {
            mLDsaParameters = MLDsaParameters.ml_dsa_87;
        }
        else
        {
            throw new InvalidProgramException($"Parameters set {publicKey.Parameters.ParameterSet.Name} is not supported.");
        }

        MLDsaKeyPairGenerator keyPairGenerator = new MLDsaKeyPairGenerator();
        keyPairGenerator.Init(new MLDsaKeyGenerationParameters(secureRandom, mLDsaParameters));
        return keyPairGenerator.GenerateKeyPair();
    }
}
