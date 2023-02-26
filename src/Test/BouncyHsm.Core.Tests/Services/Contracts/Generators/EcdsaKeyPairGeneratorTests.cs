using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.Generators;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.UseCases.Implementation;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Tests.Services.Contracts.Generators;

[TestClass]
public class EcdsaKeyPairGeneratorTests
{
    public TestContext TestContext
    {
        get;
        set;
    } = default!;

    [TestMethod]
    public void Generate_AllCurves_Success()
    {
        foreach ((byte[] ecParams, string curveName) in this.EnumerateNames())
        {
            this.TestContext.WriteLine("Start tetsing {0}.", curveName);
            EcdsaKeyPairGenerator generator = new EcdsaKeyPairGenerator(new NullLogger<EcdsaKeyPairGenerator>());

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
                {CKA.CKA_EC_PARAMS, AttributeValue.Create(ecParams) },
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

            this.TestContext.WriteLine("EC {0} success generated.", curveName);
        }
    }

    private IEnumerable<(byte[] ecParams, string curveName)> EnumerateNames()
    {
        HsmInfoFacade hsmInfoFacade = new HsmInfoFacade();
        foreach (SupportedNameCurve curve in hsmInfoFacade.GetCurves())
        {
            DerObjectIdentifier curveOid = new DerObjectIdentifier(curve.Oid);
            yield return (curveOid.GetEncoded(), curve.Name);
        }
    }
}
