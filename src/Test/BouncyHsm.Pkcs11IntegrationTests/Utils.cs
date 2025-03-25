using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Pkcs11IntegrationTests
{
    internal static class Utils
    {
        public static byte[] CreatePkcs1DigestInfo(byte[] hash, HashAlgorithmName hashAlgorithm)
        {
            if (hash == null || hash.Length == 0)
                throw new ArgumentNullException(nameof(hash));

            byte[]? pkcs1DigestInfo = null;

            if (hashAlgorithm == HashAlgorithmName.MD5)
            {
                if (hash.Length != 16)
                    throw new ArgumentException("Invalid lenght of hash value");

                pkcs1DigestInfo = new byte[] { 0x30, 0x20, 0x30, 0x0C, 0x06, 0x08, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x02, 0x05, 0x05, 0x00, 0x04, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                Array.Copy(hash, 0, pkcs1DigestInfo, pkcs1DigestInfo.Length - hash.Length, hash.Length);
            }
            else if (hashAlgorithm == HashAlgorithmName.SHA1)
            {
                if (hash.Length != 20)
                    throw new ArgumentException("Invalid lenght of hash value");

                pkcs1DigestInfo = new byte[] { 0x30, 0x21, 0x30, 0x09, 0x06, 0x05, 0x2B, 0x0E, 0x03, 0x02, 0x1A, 0x05, 0x00, 0x04, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                Array.Copy(hash, 0, pkcs1DigestInfo, pkcs1DigestInfo.Length - hash.Length, hash.Length);
            }
            else if (hashAlgorithm == HashAlgorithmName.SHA256)
            {
                if (hash.Length != 32)
                    throw new ArgumentException("Invalid lenght of hash value");

                pkcs1DigestInfo = new byte[] { 0x30, 0x31, 0x30, 0x0D, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x01, 0x05, 0x00, 0x04, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                Array.Copy(hash, 0, pkcs1DigestInfo, pkcs1DigestInfo.Length - hash.Length, hash.Length);
            }
            else if (hashAlgorithm == HashAlgorithmName.SHA384)
            {
                if (hash.Length != 48)
                    throw new ArgumentException("Invalid lenght of hash value");

                pkcs1DigestInfo = new byte[] { 0x30, 0x41, 0x30, 0x0D, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x02, 0x05, 0x00, 0x04, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                Array.Copy(hash, 0, pkcs1DigestInfo, pkcs1DigestInfo.Length - hash.Length, hash.Length);
            }
            else if (hashAlgorithm == HashAlgorithmName.SHA512)
            {
                if (hash.Length != 64)
                    throw new ArgumentException("Invalid lenght of hash value");

                pkcs1DigestInfo = new byte[] { 0x30, 0x51, 0x30, 0x0D, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x03, 0x05, 0x00, 0x04, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                Array.Copy(hash, 0, pkcs1DigestInfo, pkcs1DigestInfo.Length - hash.Length, hash.Length);
            }
            else
            {
                throw new ArgumentException("hashAlgorithm is not supported", nameof(hashAlgorithm));
            }

            return pkcs1DigestInfo;
        }

        public record EcdhData(X9ECParameters X9Parameters, ECPrivateKeyParameters EcPrivateKey, byte[] RawCertificate);
        public static EcdhData CreateEcdhParams()
        {
            X500DistinguishedName subject = new X500DistinguishedName("CN=Test");
            using ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            CertificateRequest request = new CertificateRequest(subject, ecdsa, HashAlgorithmName.SHA256);
            using X509Certificate2 certificate = request.CreateSelfSigned(DateTime.Now, DateTime.Now.AddDays(2));
            byte[] p12Data = certificate.Export(X509ContentType.Pkcs12, "Password");

            using MemoryStream ms = new MemoryStream(p12Data);

            Org.BouncyCastle.Pkcs.Pkcs12StoreBuilder stBuilder = new Org.BouncyCastle.Pkcs.Pkcs12StoreBuilder();

            Org.BouncyCastle.Pkcs.Pkcs12Store st = stBuilder.Build();
            st.Load(ms, "Password".ToCharArray());
            string? alias = st.Aliases.Cast<string>().FirstOrDefault(p => st.IsKeyEntry(p));
            Org.BouncyCastle.Pkcs.X509CertificateEntry keyEntryX = st.GetCertificate(alias);
            Org.BouncyCastle.Pkcs.AsymmetricKeyEntry keyEntry = st.GetKey(alias);
            ECPrivateKeyParameters ecPrivateKey = (ECPrivateKeyParameters)keyEntry.Key;

            X9ECParameters x9Parameters = new X9ECParameters(ecPrivateKey.Parameters.Curve,
                new X9ECPoint(ecPrivateKey.Parameters.G, false),
                ecPrivateKey.Parameters.N,
                ecPrivateKey.Parameters.H,
                ecPrivateKey.Parameters.GetSeed());

            return new EcdhData(x9Parameters, ecPrivateKey, certificate.RawData);
        }
    }
}
