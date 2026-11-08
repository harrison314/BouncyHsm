using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class HssKeyPairGenerator : IKeyPairGenerator
{
    private IReadOnlyDictionary<CKA, IAttributeValue>? publicKeyTemplate;
    private IReadOnlyDictionary<CKA, IAttributeValue>? privateKeyTemplate;
    private LmsParameters[]? lmsParameters;
    private readonly ILogger<HssKeyPairGenerator> logger;

    public HssKeyPairGenerator(ILogger<HssKeyPairGenerator> logger)
    {
        this.logger = logger;
    }

    public void Init(IReadOnlyDictionary<CKA, IAttributeValue> publicKeyTemplate, IReadOnlyDictionary<CKA, IAttributeValue> privateKeyTemplate)
    {
        this.logger.LogTrace("Entering to Init");

        this.CheckFlags(privateKeyTemplate);

        uint levels = privateKeyTemplate.GetRequiredAttributeUint(CKA.CKA_HSS_LEVELS);
        uint[] lmsTypes = GetRequiredUintArray(privateKeyTemplate, CKA.CKA_HSS_LMS_TYPES);
        uint[] lmotsTypes = GetRequiredUintArray(privateKeyTemplate, CKA.CKA_HSS_LMOTS_TYPES);

        this.lmsParameters = HssUtils.CreateParameters(levels, lmsTypes, lmotsTypes);
        this.publicKeyTemplate = publicKeyTemplate;
        this.privateKeyTemplate = privateKeyTemplate;
    }

    public (PublicKeyObject publicKey, PrivateKeyObject privateKey) Generate(SecureRandom secureRandom)
    {
        this.logger.LogTrace("Entering to Generate");

        System.Diagnostics.Debug.Assert(this.publicKeyTemplate != null);
        System.Diagnostics.Debug.Assert(this.privateKeyTemplate != null);
        System.Diagnostics.Debug.Assert(this.lmsParameters != null);

        Org.BouncyCastle.Pqc.Crypto.Lms.HssKeyPairGenerator generator = new Org.BouncyCastle.Pqc.Crypto.Lms.HssKeyPairGenerator();
        generator.Init(new HssKeyGenerationParameters(this.lmsParameters, secureRandom));
        AsymmetricCipherKeyPair keyPair = generator.GenerateKeyPair();

        this.logger.LogInformation("Generated HSS key pair ({levels} levels).", this.lmsParameters.Length);

        HssPublicKeyObject pubKeyObject = new HssPublicKeyObject();
        CopyTemplate(pubKeyObject, this.publicKeyTemplate);
        pubKeyObject.SetPublicKey(keyPair.Public);

        HssPrivateKeyObject privKeyObject = new HssPrivateKeyObject();
        CopyTemplate(privKeyObject, this.privateKeyTemplate);
        privKeyObject.SetPrivateKey(keyPair.Private);

        if (this.privateKeyTemplate.ContainsKey(CKA.CKA_ALLOWED_MECHANISMS) && !this.publicKeyTemplate.ContainsKey(CKA.CKA_ALLOWED_MECHANISMS))
        {
            pubKeyObject.CkaAllovedMechanism = privKeyObject.CkaAllovedMechanism;
        }

        return (pubKeyObject, privKeyObject);
    }

    public override string ToString()
    {
        return "HssKeyPairGenerator";
    }

    private static void CopyTemplate(StorageObject target, IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        foreach ((CKA attrType, IAttributeValue attrValue) in template)
        {
            if (attrType is CKA.CKA_CLASS or CKA.CKA_KEY_TYPE or CKA.CKA_HSS_LEVELS
                or CKA.CKA_HSS_LMS_TYPES or CKA.CKA_HSS_LMOTS_TYPES
                or CKA.CKA_HSS_LMS_TYPE or CKA.CKA_HSS_LMOTS_TYPE)
            {
                continue;
            }

            target.SetValue(attrType, attrValue, false);
        }
    }

    private static uint[] GetRequiredUintArray(IReadOnlyDictionary<CKA, IAttributeValue> template, CKA attributeType)
    {
        if (template.TryGetValue(attributeType, out IAttributeValue? value))
        {
            if (value.TypeTag != AttrTypeTag.UintArray)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, $"Attribute {attributeType} must be CK_ULONG array.");
            }

            return value.AsUintArray();
        }

        throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCOMPLETE, $"Missing attribute {attributeType}.");
    }

    private void CheckFlags(IReadOnlyDictionary<CKA, IAttributeValue> privateKeyTemplate)
    {
        CheckBool(privateKeyTemplate, CKA.CKA_EXTRACTABLE, true);
        CheckBool(privateKeyTemplate, CKA.CKA_SENSITIVE, false);
        CheckBool(privateKeyTemplate, CKA.CKA_COPYABLE, true);
    }

    private static void CheckBool(IReadOnlyDictionary<CKA, IAttributeValue> template, CKA attributeType, bool forbidden)
    {
        if (template.TryGetValue(attributeType, out IAttributeValue? value)
            && value.TypeTag == AttrTypeTag.CkBool
            && value.AsBool() == forbidden)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
                $"Attribute {attributeType}={forbidden} is not allowed for HSS private key.");
        }
    }
}
