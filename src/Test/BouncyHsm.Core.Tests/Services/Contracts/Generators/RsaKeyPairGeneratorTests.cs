using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.Generators;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;

namespace BouncyHsm.Core.Tests.Services.Contracts.Generators;

[TestClass]
public class RsaKeyPairGeneratorTests
{
    [DataTestMethod]
    //[DataRow(1024)]
    [DataRow(2048)]
    [DataRow(3072)]
    [DataRow(4096)]
    [DataRow(5120)]
    public void Generate_WithSize_Success(int keySize)
    {
        RsaKeyPairGenerator generator = new RsaKeyPairGenerator(new NullLogger<RsaKeyPairGenerator>());

        Dictionary<CKA, IAttributeValue> publicKeyTemplate = new Dictionary<CKA, IAttributeValue>()
        {
            {CKA.CKA_TOKEN, AttributeValue.Create(true) },
            {CKA.CKA_PRIVATE, AttributeValue.Create(false) },
            {CKA.CKA_ID, AttributeValue.Create(new byte[]{ 1, 2, 3, 4 }) },
            {CKA.CKA_LABEL, AttributeValue.Create("hello") },
            {CKA.CKA_ENCRYPT, AttributeValue.Create(false) },
            {CKA.CKA_VERIFY, AttributeValue.Create(true) },
            {CKA.CKA_VERIFY_RECOVER, AttributeValue.Create(false) },
            {CKA.CKA_WRAP, AttributeValue.Create(false) },
            {CKA.CKA_MODULUS_BITS, AttributeValue.Create((uint)keySize) },
            {CKA.CKA_PUBLIC_EXPONENT, AttributeValue.Create(new byte[]{1,0,1}) },
            {CKA.CKA_DESTROYABLE, AttributeValue.Create(true) },
            {CKA.CKA_MODIFIABLE, AttributeValue.Create(true) }
        };

        Dictionary<CKA, IAttributeValue> privateKeyTemplate = new Dictionary<CKA, IAttributeValue>()
            {
                {CKA.CKA_TOKEN, AttributeValue.Create(true) },
                {CKA.CKA_PRIVATE, AttributeValue.Create(true) },
                {CKA.CKA_ID, AttributeValue.Create(new byte[]{ 1, 2, 3, 4 }) },
                {CKA.CKA_LABEL, AttributeValue.Create("hello") },
                {CKA.CKA_SENSITIVE, AttributeValue.Create(false) },
                {CKA.CKA_EXTRACTABLE, AttributeValue.Create(false) },
                {CKA.CKA_DECRYPT, AttributeValue.Create(false) },
                {CKA.CKA_SIGN, AttributeValue.Create(true) },
                {CKA.CKA_SIGN_RECOVER, AttributeValue.Create(false) },
                {CKA.CKA_UNWRAP, AttributeValue.Create(false) },
                {CKA.CKA_DESTROYABLE, AttributeValue.Create(true) },
                {CKA.CKA_MODIFIABLE, AttributeValue.Create(true) }
            };

        generator.Init(publicKeyTemplate, privateKeyTemplate);
        (PublicKeyObject pubKey, PrivateKeyObject privKey) = generator.Generate(new Org.BouncyCastle.Security.SecureRandom());

        pubKey.ReComputeAttributes();
        privKey.ReComputeAttributes();

        pubKey.Validate();
        privKey.Validate();
    }
}
