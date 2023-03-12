using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.Generators;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging.Abstractions;

namespace BouncyHsm.Core.Tests.Services.Contracts.Generators;

[TestClass]
public class GenericSecretKeyGeneratorTests
{
    [DataTestMethod]
    [DataRow(CKK.CKK_GENERIC_SECRET, 12)]
    [DataRow(CKK.CKK_GENERIC_SECRET, 256)]
    [DataRow(CKK.CKK_MD5_HMAC, 20)]
    [DataRow(CKK.CKK_RIPEMD128_HMAC, 56)]
    [DataRow(CKK.CKK_RIPEMD160_HMAC, 20)]
    [DataRow(CKK.CKK_SHA224_HMAC, 32)]
    [DataRow(CKK.CKK_SHA256_HMAC, 32)]
    [DataRow(CKK.CKK_SHA256_HMAC, 156)]
    [DataRow(CKK.CKK_SHA384_HMAC, 64)]
    [DataRow(CKK.CKK_SHA512_HMAC, 64)]
    [DataRow(CKK.CKK_SHA_1_HMAC, 20)]
    public void Generate_CallWithLen_Success(CKK keyType, int size)
    {
        GenericSecretKeyGenerator generator = new GenericSecretKeyGenerator(new NullLogger<GenericSecretKeyGenerator>());
        Dictionary<CKA, IAttributeValue> template = new Dictionary<CKA, IAttributeValue>()
        {
            {CKA.CKA_KEY_TYPE, AttributeValue.Create((uint)keyType) },
            {CKA.CKA_TOKEN, AttributeValue.Create(true) },
            {CKA.CKA_PRIVATE, AttributeValue.Create(false) },
            {CKA.CKA_ID, AttributeValue.Create(new byte[]{ 1, 2, 3, 4 }) },
            {CKA.CKA_LABEL, AttributeValue.Create("hello") },
            {CKA.CKA_ENCRYPT, AttributeValue.Create(true) },
            {CKA.CKA_VERIFY, AttributeValue.Create(true) },
            {CKA.CKA_WRAP, AttributeValue.Create(false) },
            {CKA.CKA_DESTROYABLE, AttributeValue.Create(true) },
            {CKA.CKA_MODIFIABLE, AttributeValue.Create(true) },
            {CKA.CKA_VALUE_LEN, AttributeValue.Create((uint)size) }
        };

        generator.Init(template);
        SecretKeyObject key = generator.Generate(new Org.BouncyCastle.Security.SecureRandom());

        key.ReComputeAttributes();
        key.Validate();

        Assert.AreEqual(size, key.GetSecret().Length);
    }
}