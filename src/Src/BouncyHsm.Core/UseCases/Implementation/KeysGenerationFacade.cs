using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.Generators;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.UseCases.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.UseCases.Implementation;

public class KeysGenerationFacade : IKeysGenerationFacade
{
    private readonly IPersistentRepository persistentRepository;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<KeysGenerationFacade> logger;

    public KeysGenerationFacade(IPersistentRepository persistentRepository,
        ILoggerFactory loggerFactory,
        ILogger<KeysGenerationFacade> logger)
    {
        this.persistentRepository = persistentRepository;
        this.loggerFactory = loggerFactory;
        this.logger = logger;
    }

    public async Task<DomainResult<GeneratedKeyPairIds>> GenerateRsaKeyPair(uint slotId, GenerateRsaKeyPairRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GenerateRsaKeyPair with slotId {slotId}, keySize {keySize}, label {label}.", slotId, request.KeySize, request.KeyAttributes.CkaLabel);

        SlotEntity? slotEntity = await this.persistentRepository.GetSlot(slotId, cancellationToken);
        if (slotEntity == null)
        {
            this.logger.LogError("Parameter slotId {slotId} is invalid.", slotId);
            return new DomainResult<GeneratedKeyPairIds>.InvalidInput("Invalid slotId.");
        }

        if (request.KeySize % 1024 != 0)
        {
            this.logger.LogError("Parameter KeySize {KeySize} is invalid.", request.KeySize);
            return new DomainResult<GeneratedKeyPairIds>.InvalidInput("Invalid key size.");
        }

        if (request.KeyAttributes.ForDerivation)
        {
            this.logger.LogError("RSA keys can not support derivation.");
            return new DomainResult<GeneratedKeyPairIds>.InvalidInput("RSA keys can not support derivation.");
        }

        byte[] ckaId = request.KeyAttributes.CkaId ?? RandomNumberGenerator.GetBytes(32);

        Dictionary<CKA, IAttributeValue> publicKeyTemplate = new Dictionary<CKA, IAttributeValue>()
        {
            { CKA.CKA_TOKEN, AttributeValue.Create(true) },
            { CKA.CKA_DESTROYABLE, AttributeValue.Create(true) },
            { CKA.CKA_PRIVATE, AttributeValue.Create(false) },
            { CKA.CKA_LABEL, AttributeValue.Create(request.KeyAttributes.CkaLabel) },
            { CKA.CKA_ID, AttributeValue.Create(ckaId) },
            { CKA.CKA_ENCRYPT, AttributeValue.Create(request.KeyAttributes.ForEncryption) },
            { CKA.CKA_VERIFY, AttributeValue.Create(request.KeyAttributes.ForSigning) },
            { CKA.CKA_VERIFY_RECOVER, AttributeValue.Create(request.KeyAttributes.ForSigning) },
            { CKA.CKA_WRAP, AttributeValue.Create(request.KeyAttributes.ForWrap) },
            { CKA.CKA_MODULUS_BITS, AttributeValue.Create((uint)request.KeySize) },
            { CKA.CKA_PUBLIC_EXPONENT, AttributeValue.Create(new byte[] { 0x01, 0x00, 0x01 }) }
        };

        Dictionary<CKA, IAttributeValue> privateKeyTemplate = new Dictionary<CKA, IAttributeValue>()
        {
            { CKA.CKA_TOKEN, AttributeValue.Create(true) },
            { CKA.CKA_DESTROYABLE, AttributeValue.Create(true) },
            { CKA.CKA_PRIVATE, AttributeValue.Create(true) },
            { CKA.CKA_LABEL, AttributeValue.Create(request.KeyAttributes.CkaLabel) },
            { CKA.CKA_ID, AttributeValue.Create(ckaId) },
            { CKA.CKA_SENSITIVE, AttributeValue.Create(request.KeyAttributes.Sensitive) },
            { CKA.CKA_EXTRACTABLE, AttributeValue.Create(request.KeyAttributes.Exportable) },
            { CKA.CKA_DECRYPT, AttributeValue.Create(request.KeyAttributes.ForEncryption) },
            { CKA.CKA_SIGN, AttributeValue.Create(request.KeyAttributes.ForSigning) },
            { CKA.CKA_SIGN_RECOVER, AttributeValue.Create(request.KeyAttributes.ForSigning) },
            { CKA.CKA_UNWRAP, AttributeValue.Create(request.KeyAttributes.ForWrap) },
        };

        RsaKeyPairGenerator rsaGenerator = new RsaKeyPairGenerator(false, this.loggerFactory.CreateLogger<RsaKeyPairGenerator>());
        rsaGenerator.Init(publicKeyTemplate, privateKeyTemplate);

        (PublicKeyObject publicKey, PrivateKeyObject privateKey) = rsaGenerator.Generate(BouncyHsm.Core.Services.Bc.HwRandomGenerator.SecureRandom);

        this.UpdateKeys(slotEntity, publicKey, privateKey);

        publicKey.Validate();
        privateKey.Validate();

        await this.persistentRepository.StoreObject(slotId, publicKey, cancellationToken);
        await this.persistentRepository.StoreObject(slotId, privateKey, cancellationToken);

        return new DomainResult<GeneratedKeyPairIds>.Ok(new GeneratedKeyPairIds(publicKey.Id, privateKey.Id));
    }

    public async Task<DomainResult<GeneratedKeyPairIds>> GenerateEcKeyPair(uint slotId, GenerateEcKeyPairRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GenerateEcKeyPair with slotId {slotId}, OID or name {oidOrName}, label {label}.", slotId, request.OidOrName, request.KeyAttributes.CkaLabel);

        SlotEntity? slotEntity = await this.persistentRepository.GetSlot(slotId, cancellationToken);
        if (slotEntity == null)
        {
            this.logger.LogError("Parameter slotId {slotId} is invalid.", slotId);
            return new DomainResult<GeneratedKeyPairIds>.InvalidInput("Invalid slotId.");
        }

        byte[] ckaId = request.KeyAttributes.CkaId ?? RandomNumberGenerator.GetBytes(32);
        byte[] namedCurveOid;
        try
        {
            namedCurveOid = EcdsaUtils.GetEcOidFromNameOrOid(request.OidOrName).GetEncoded();
        }
        catch (ArgumentException ex)
        {
            this.logger.LogError(ex, "Parameter OidOrName {OidOrName} is invalid.", request.OidOrName);
            return new DomainResult<GeneratedKeyPairIds>.InvalidInput("Invalid OID or curve name.");
        }

        Dictionary<CKA, IAttributeValue> publicKeyTemplate = new Dictionary<CKA, IAttributeValue>()
        {
            { CKA.CKA_TOKEN, AttributeValue.Create(true) },
            { CKA.CKA_DESTROYABLE, AttributeValue.Create(true) },
            { CKA.CKA_PRIVATE, AttributeValue.Create(false) },
            { CKA.CKA_LABEL, AttributeValue.Create(request.KeyAttributes.CkaLabel) },
            { CKA.CKA_ID, AttributeValue.Create(ckaId) },
            { CKA.CKA_ENCRYPT, AttributeValue.Create(request.KeyAttributes.ForEncryption) },
            { CKA.CKA_VERIFY, AttributeValue.Create(request.KeyAttributes.ForSigning) },
            { CKA.CKA_VERIFY_RECOVER, AttributeValue.Create(request.KeyAttributes.ForSigning) },
            { CKA.CKA_WRAP, AttributeValue.Create(request.KeyAttributes.ForWrap) },
            { CKA.CKA_EC_PARAMS, AttributeValue.Create(namedCurveOid) },
        };

        Dictionary<CKA, IAttributeValue> privateKeyTemplate = new Dictionary<CKA, IAttributeValue>()
        {
            { CKA.CKA_TOKEN, AttributeValue.Create(true) },
            { CKA.CKA_DESTROYABLE, AttributeValue.Create(true) },
            { CKA.CKA_PRIVATE, AttributeValue.Create(true) },
            { CKA.CKA_LABEL, AttributeValue.Create(request.KeyAttributes.CkaLabel) },
            { CKA.CKA_ID, AttributeValue.Create(ckaId) },
            { CKA.CKA_SENSITIVE, AttributeValue.Create(request.KeyAttributes.Sensitive) },
            { CKA.CKA_EXTRACTABLE, AttributeValue.Create(request.KeyAttributes.Exportable) },
            { CKA.CKA_DECRYPT, AttributeValue.Create(request.KeyAttributes.ForEncryption) },
            { CKA.CKA_SIGN, AttributeValue.Create(request.KeyAttributes.ForSigning) },
            { CKA.CKA_SIGN_RECOVER, AttributeValue.Create(request.KeyAttributes.ForSigning) },
            { CKA.CKA_UNWRAP, AttributeValue.Create(request.KeyAttributes.ForWrap) },
            { CKA.CKA_DERIVE, AttributeValue.Create(request.KeyAttributes.ForDerivation) },
        };

        EcdsaKeyPairGenerator ecdsaKeyPairGenerator = new EcdsaKeyPairGenerator(this.loggerFactory.CreateLogger<EcdsaKeyPairGenerator>());
        ecdsaKeyPairGenerator.Init(publicKeyTemplate, privateKeyTemplate);
        (PublicKeyObject publicKey, PrivateKeyObject privateKey) = ecdsaKeyPairGenerator.Generate(BouncyHsm.Core.Services.Bc.HwRandomGenerator.SecureRandom);

        this.UpdateKeys(slotEntity, publicKey, privateKey);

        publicKey.Validate();
        privateKey.Validate();

        await this.persistentRepository.StoreObject(slotId, publicKey, cancellationToken);
        await this.persistentRepository.StoreObject(slotId, privateKey, cancellationToken);

        return new DomainResult<GeneratedKeyPairIds>.Ok(new GeneratedKeyPairIds(publicKey.Id, privateKey.Id));
    }

    public async Task<DomainResult<GeneratedKeyPairIds>> GenerateEdwardsKeyPair(uint slotId, GenerateEdwardsKeyPairRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GenerateEdwardsKeyPair with slotId {slotId}, OID or name {oidOrName}, label {label}.", slotId, request.OidOrName, request.KeyAttributes.CkaLabel);

        SlotEntity? slotEntity = await this.persistentRepository.GetSlot(slotId, cancellationToken);
        if (slotEntity == null)
        {
            this.logger.LogError("Parameter slotId {slotId} is invalid.", slotId);
            return new DomainResult<GeneratedKeyPairIds>.InvalidInput("Invalid slotId.");
        }

        byte[] ckaId = request.KeyAttributes.CkaId ?? RandomNumberGenerator.GetBytes(32);
        byte[] ecParams;
        try
        {
            ecParams = EdEcUtils.CreateEcparam(request.OidOrName);
        }
        catch (ArgumentException ex)
        {
            this.logger.LogError(ex, "Parameter OidOrName {OidOrName} is invalid.", request.OidOrName);
            return new DomainResult<GeneratedKeyPairIds>.InvalidInput("Invalid OID or curve name.");
        }

        Dictionary<CKA, IAttributeValue> publicKeyTemplate = new Dictionary<CKA, IAttributeValue>()
        {
            { CKA.CKA_TOKEN, AttributeValue.Create(true) },
            { CKA.CKA_DESTROYABLE, AttributeValue.Create(true) },
            { CKA.CKA_PRIVATE, AttributeValue.Create(false) },
            { CKA.CKA_LABEL, AttributeValue.Create(request.KeyAttributes.CkaLabel) },
            { CKA.CKA_ID, AttributeValue.Create(ckaId) },
            { CKA.CKA_ENCRYPT, AttributeValue.Create(request.KeyAttributes.ForEncryption) },
            { CKA.CKA_VERIFY, AttributeValue.Create(request.KeyAttributes.ForSigning) },
            { CKA.CKA_VERIFY_RECOVER, AttributeValue.Create(request.KeyAttributes.ForSigning) },
            { CKA.CKA_WRAP, AttributeValue.Create(request.KeyAttributes.ForWrap) },
            { CKA.CKA_EC_PARAMS, AttributeValue.Create(ecParams) },
        };

        Dictionary<CKA, IAttributeValue> privateKeyTemplate = new Dictionary<CKA, IAttributeValue>()
        {
            { CKA.CKA_TOKEN, AttributeValue.Create(true) },
            { CKA.CKA_DESTROYABLE, AttributeValue.Create(true) },
            { CKA.CKA_PRIVATE, AttributeValue.Create(true) },
            { CKA.CKA_LABEL, AttributeValue.Create(request.KeyAttributes.CkaLabel) },
            { CKA.CKA_ID, AttributeValue.Create(ckaId) },
            { CKA.CKA_SENSITIVE, AttributeValue.Create(request.KeyAttributes.Sensitive) },
            { CKA.CKA_EXTRACTABLE, AttributeValue.Create(request.KeyAttributes.Exportable) },
            { CKA.CKA_DECRYPT, AttributeValue.Create(request.KeyAttributes.ForEncryption) },
            { CKA.CKA_SIGN, AttributeValue.Create(request.KeyAttributes.ForSigning) },
            { CKA.CKA_SIGN_RECOVER, AttributeValue.Create(request.KeyAttributes.ForSigning) },
            { CKA.CKA_UNWRAP, AttributeValue.Create(request.KeyAttributes.ForWrap) },
            { CKA.CKA_DERIVE, AttributeValue.Create(request.KeyAttributes.ForDerivation) },
        };

        EdwardsKeyPairGenerator edwardsKeyPairGenerator = new EdwardsKeyPairGenerator(this.loggerFactory.CreateLogger<EdwardsKeyPairGenerator>());
        edwardsKeyPairGenerator.Init(publicKeyTemplate, privateKeyTemplate);
        (PublicKeyObject publicKey, PrivateKeyObject privateKey) = edwardsKeyPairGenerator.Generate(BouncyHsm.Core.Services.Bc.HwRandomGenerator.SecureRandom);

        this.UpdateKeys(slotEntity, publicKey, privateKey);

        publicKey.Validate();
        privateKey.Validate();

        await this.persistentRepository.StoreObject(slotId, publicKey, cancellationToken);
        await this.persistentRepository.StoreObject(slotId, privateKey, cancellationToken);

        return new DomainResult<GeneratedKeyPairIds>.Ok(new GeneratedKeyPairIds(publicKey.Id, privateKey.Id));
    }

    public async Task<DomainResult<GeneratedSecretId>> GenerateSecretKey(uint slotId, GenerateSecretKeyRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GenerateSecretKey with slotId {slotId}, keySize {keySize}, label {label}.", slotId, request.Size, request.KeyAttributes.CkaLabel);

        if (await this.persistentRepository.GetSlot(slotId, cancellationToken) == null)
        {
            this.logger.LogError("Parameter slotId {slotId} is invalid.", slotId);
            return new DomainResult<GeneratedSecretId>.InvalidInput("Invalid slotId.");
        }

        byte[] ckaId = request.KeyAttributes.CkaId ?? RandomNumberGenerator.GetBytes(32);
        Dictionary<CKA, IAttributeValue> template = new Dictionary<CKA, IAttributeValue>()
        {
            { CKA.CKA_CLASS, AttributeValue.Create((uint)CKO.CKO_SECRET_KEY) },
            { CKA.CKA_KEY_TYPE, AttributeValue.Create((uint)CKK.CKK_GENERIC_SECRET) },
            { CKA.CKA_TOKEN, AttributeValue.Create(true) },
            { CKA.CKA_DESTROYABLE, AttributeValue.Create(true) },
            { CKA.CKA_PRIVATE, AttributeValue.Create(true) },
            { CKA.CKA_LABEL, AttributeValue.Create(request.KeyAttributes.CkaLabel) },
            { CKA.CKA_ID, AttributeValue.Create(ckaId) },
            { CKA.CKA_ENCRYPT, AttributeValue.Create(request.KeyAttributes.ForEncryption) },
            { CKA.CKA_DECRYPT, AttributeValue.Create(request.KeyAttributes.ForEncryption) },
            { CKA.CKA_VERIFY, AttributeValue.Create(request.KeyAttributes.ForSigning) },
            { CKA.CKA_SIGN, AttributeValue.Create(request.KeyAttributes.ForSigning) },

            { CKA.CKA_SENSITIVE, AttributeValue.Create(request.KeyAttributes.Sensitive) },
            { CKA.CKA_EXTRACTABLE, AttributeValue.Create(request.KeyAttributes.Exportable) },

            { CKA.CKA_WRAP, AttributeValue.Create(request.KeyAttributes.ForWrap) },
            { CKA.CKA_UNWRAP, AttributeValue.Create(request.KeyAttributes.ForWrap) },

            { CKA.CKA_VALUE_LEN, AttributeValue.Create((uint) request.Size) },
            { CKA.CKA_DERIVE, AttributeValue.Create(request.KeyAttributes.ForDerivation) },
        };

        GenericSecretKeyGenerator generator = new GenericSecretKeyGenerator(this.loggerFactory.CreateLogger<GenericSecretKeyGenerator>());
        generator.Init(template);
        SecretKeyObject secretKeyObject = generator.Generate(BouncyHsm.Core.Services.Bc.HwRandomGenerator.SecureRandom);

        this.UpdateKey(secretKeyObject);
        secretKeyObject.Validate();

        await this.persistentRepository.StoreObject(slotId, secretKeyObject, cancellationToken);

        return new DomainResult<GeneratedSecretId>.Ok(new GeneratedSecretId(secretKeyObject.Id));
    }

    public async Task<DomainResult<GeneratedSecretId>> GenerateAesKey(uint slotId, GenerateAesKeyRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GenerateAesKey with slotId {slotId}, keySize {keySize}, label {label}.", slotId, request.Size, request.KeyAttributes.CkaLabel);

        if (await this.persistentRepository.GetSlot(slotId, cancellationToken) == null)
        {
            this.logger.LogError("Parameter slotId {slotId} is invalid.", slotId);
            return new DomainResult<GeneratedSecretId>.InvalidInput("Invalid slotId.");
        }

        if (request.KeyAttributes.ForDerivation)
        {
            this.logger.LogError("AES keys can not support derivation.");
            return new DomainResult<GeneratedSecretId>.InvalidInput("AES keys can not support derivation.");
        }

        byte[] ckaId = request.KeyAttributes.CkaId ?? RandomNumberGenerator.GetBytes(32);
        Dictionary<CKA, IAttributeValue> template = new Dictionary<CKA, IAttributeValue>()
        {
            { CKA.CKA_TOKEN, AttributeValue.Create(true) },
            { CKA.CKA_DESTROYABLE, AttributeValue.Create(true) },
            { CKA.CKA_PRIVATE, AttributeValue.Create(true) },
            { CKA.CKA_LABEL, AttributeValue.Create(request.KeyAttributes.CkaLabel) },
            { CKA.CKA_ID, AttributeValue.Create(ckaId) },
            { CKA.CKA_ENCRYPT, AttributeValue.Create(request.KeyAttributes.ForEncryption) },
            { CKA.CKA_DECRYPT, AttributeValue.Create(request.KeyAttributes.ForEncryption) },
            { CKA.CKA_VERIFY, AttributeValue.Create(request.KeyAttributes.ForSigning) },
            { CKA.CKA_SIGN, AttributeValue.Create(request.KeyAttributes.ForSigning) },

            { CKA.CKA_SENSITIVE, AttributeValue.Create(request.KeyAttributes.Sensitive) },
            { CKA.CKA_EXTRACTABLE, AttributeValue.Create(request.KeyAttributes.Exportable) },

            { CKA.CKA_WRAP, AttributeValue.Create(request.KeyAttributes.ForWrap) },
            { CKA.CKA_UNWRAP, AttributeValue.Create(request.KeyAttributes.ForWrap) },

            { CKA.CKA_VALUE_LEN, AttributeValue.Create((uint) request.Size) },
        };

        AesKeyGenerator generator = new AesKeyGenerator(this.loggerFactory.CreateLogger<AesKeyGenerator>());
        generator.Init(template);
        SecretKeyObject secretKeyObject = generator.Generate(BouncyHsm.Core.Services.Bc.HwRandomGenerator.SecureRandom);

        this.UpdateKey(secretKeyObject);
        secretKeyObject.Validate();

        await this.persistentRepository.StoreObject(slotId, secretKeyObject, cancellationToken);

        return new DomainResult<GeneratedSecretId>.Ok(new GeneratedSecretId(secretKeyObject.Id));
    }

    public async Task<DomainResult<GeneratedSecretId>> GeneratePoly1305Key(uint slotId, GeneratePoly1305KeyRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GeneratePoly1305Key with slotId {slotId}, label {label}.", slotId, request.KeyAttributes.CkaLabel);

        if (await this.persistentRepository.GetSlot(slotId, cancellationToken) == null)
        {
            this.logger.LogError("Parameter slotId {slotId} is invalid.", slotId);
            return new DomainResult<GeneratedSecretId>.InvalidInput("Invalid slotId.");
        }

        if (request.KeyAttributes.ForDerivation)
        {
            this.logger.LogError("Poly1305 keys can not support derivation.");
            return new DomainResult<GeneratedSecretId>.InvalidInput("Poly1305 keys can not support derivation.");
        }

        byte[] ckaId = request.KeyAttributes.CkaId ?? RandomNumberGenerator.GetBytes(32);
        Dictionary<CKA, IAttributeValue> template = new Dictionary<CKA, IAttributeValue>()
        {
            { CKA.CKA_TOKEN, AttributeValue.Create(true) },
            { CKA.CKA_DESTROYABLE, AttributeValue.Create(true) },
            { CKA.CKA_PRIVATE, AttributeValue.Create(true) },
            { CKA.CKA_LABEL, AttributeValue.Create(request.KeyAttributes.CkaLabel) },
            { CKA.CKA_ID, AttributeValue.Create(ckaId) },
            { CKA.CKA_ENCRYPT, AttributeValue.Create(request.KeyAttributes.ForEncryption) },
            { CKA.CKA_DECRYPT, AttributeValue.Create(request.KeyAttributes.ForEncryption) },
            { CKA.CKA_VERIFY, AttributeValue.Create(request.KeyAttributes.ForSigning) },
            { CKA.CKA_SIGN, AttributeValue.Create(request.KeyAttributes.ForSigning) },

            { CKA.CKA_SENSITIVE, AttributeValue.Create(request.KeyAttributes.Sensitive) },
            { CKA.CKA_EXTRACTABLE, AttributeValue.Create(request.KeyAttributes.Exportable) },

            { CKA.CKA_WRAP, AttributeValue.Create(request.KeyAttributes.ForWrap) },
            { CKA.CKA_UNWRAP, AttributeValue.Create(request.KeyAttributes.ForWrap) },

            { CKA.CKA_VALUE_LEN, AttributeValue.Create(32) },
        };

        Poly1305KeyGenerator generator = new Poly1305KeyGenerator(this.loggerFactory.CreateLogger<Poly1305KeyGenerator>());
        generator.Init(template);
        SecretKeyObject secretKeyObject = generator.Generate(BouncyHsm.Core.Services.Bc.HwRandomGenerator.SecureRandom);

        this.UpdateKey(secretKeyObject);
        secretKeyObject.Validate();

        await this.persistentRepository.StoreObject(slotId, secretKeyObject, cancellationToken);

        return new DomainResult<GeneratedSecretId>.Ok(new GeneratedSecretId(secretKeyObject.Id));
    }

    public async Task<DomainResult<GeneratedSecretId>> GenerateChaCha20Key(uint slotId, GenerateChaCha20KeyRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GenerateChaCha20Key with slotId {slotId}, label {label}.", slotId, request.KeyAttributes.CkaLabel);

        if (await this.persistentRepository.GetSlot(slotId, cancellationToken) == null)
        {
            this.logger.LogError("Parameter slotId {slotId} is invalid.", slotId);
            return new DomainResult<GeneratedSecretId>.InvalidInput("Invalid slotId.");
        }

        if (request.KeyAttributes.ForDerivation)
        {
            this.logger.LogError("ChaCha20 keys can not support derivation.");
            return new DomainResult<GeneratedSecretId>.InvalidInput("ChaCha20 keys can not support derivation.");
        }

        byte[] ckaId = request.KeyAttributes.CkaId ?? RandomNumberGenerator.GetBytes(32);
        Dictionary<CKA, IAttributeValue> template = new Dictionary<CKA, IAttributeValue>()
        {
            { CKA.CKA_TOKEN, AttributeValue.Create(true) },
            { CKA.CKA_DESTROYABLE, AttributeValue.Create(true) },
            { CKA.CKA_PRIVATE, AttributeValue.Create(true) },
            { CKA.CKA_LABEL, AttributeValue.Create(request.KeyAttributes.CkaLabel) },
            { CKA.CKA_ID, AttributeValue.Create(ckaId) },
            { CKA.CKA_ENCRYPT, AttributeValue.Create(request.KeyAttributes.ForEncryption) },
            { CKA.CKA_DECRYPT, AttributeValue.Create(request.KeyAttributes.ForEncryption) },
            { CKA.CKA_VERIFY, AttributeValue.Create(request.KeyAttributes.ForSigning) },
            { CKA.CKA_SIGN, AttributeValue.Create(request.KeyAttributes.ForSigning) },

            { CKA.CKA_SENSITIVE, AttributeValue.Create(request.KeyAttributes.Sensitive) },
            { CKA.CKA_EXTRACTABLE, AttributeValue.Create(request.KeyAttributes.Exportable) },

            { CKA.CKA_WRAP, AttributeValue.Create(request.KeyAttributes.ForWrap) },
            { CKA.CKA_UNWRAP, AttributeValue.Create(request.KeyAttributes.ForWrap) },

            { CKA.CKA_VALUE_LEN, AttributeValue.Create(32) },
        };

        ChaCha20KeyGenerator generator = new ChaCha20KeyGenerator(this.loggerFactory.CreateLogger<ChaCha20KeyGenerator>());
        generator.Init(template);
        SecretKeyObject secretKeyObject = generator.Generate(BouncyHsm.Core.Services.Bc.HwRandomGenerator.SecureRandom);

        this.UpdateKey(secretKeyObject);
        secretKeyObject.Validate();

        await this.persistentRepository.StoreObject(slotId, secretKeyObject, cancellationToken);

        return new DomainResult<GeneratedSecretId>.Ok(new GeneratedSecretId(secretKeyObject.Id));
    }

    public async Task<DomainResult<GeneratedSecretId>> GenerateSalsa20Key(uint slotId, GenerateSalsa20KeyRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GenerateSalsa20Key with slotId {slotId}, label {label}.", slotId, request.KeyAttributes.CkaLabel);

        if (await this.persistentRepository.GetSlot(slotId, cancellationToken) == null)
        {
            this.logger.LogError("Parameter slotId {slotId} is invalid.", slotId);
            return new DomainResult<GeneratedSecretId>.InvalidInput("Invalid slotId.");
        }

        if (request.KeyAttributes.ForDerivation)
        {
            this.logger.LogError("Salsa20 keys can not support derivation.");
            return new DomainResult<GeneratedSecretId>.InvalidInput("Salsa20 keys can not support derivation.");
        }

        byte[] ckaId = request.KeyAttributes.CkaId ?? RandomNumberGenerator.GetBytes(32);
        Dictionary<CKA, IAttributeValue> template = new Dictionary<CKA, IAttributeValue>()
        {
            { CKA.CKA_TOKEN, AttributeValue.Create(true) },
            { CKA.CKA_DESTROYABLE, AttributeValue.Create(true) },
            { CKA.CKA_PRIVATE, AttributeValue.Create(true) },
            { CKA.CKA_LABEL, AttributeValue.Create(request.KeyAttributes.CkaLabel) },
            { CKA.CKA_ID, AttributeValue.Create(ckaId) },
            { CKA.CKA_ENCRYPT, AttributeValue.Create(request.KeyAttributes.ForEncryption) },
            { CKA.CKA_DECRYPT, AttributeValue.Create(request.KeyAttributes.ForEncryption) },
            { CKA.CKA_VERIFY, AttributeValue.Create(request.KeyAttributes.ForSigning) },
            { CKA.CKA_SIGN, AttributeValue.Create(request.KeyAttributes.ForSigning) },

            { CKA.CKA_SENSITIVE, AttributeValue.Create(request.KeyAttributes.Sensitive) },
            { CKA.CKA_EXTRACTABLE, AttributeValue.Create(request.KeyAttributes.Exportable) },

            { CKA.CKA_WRAP, AttributeValue.Create(request.KeyAttributes.ForWrap) },
            { CKA.CKA_UNWRAP, AttributeValue.Create(request.KeyAttributes.ForWrap) },

            { CKA.CKA_VALUE_LEN, AttributeValue.Create(32) },
        };

        Salsa20KeyGenerator generator = new Salsa20KeyGenerator(this.loggerFactory.CreateLogger<Salsa20KeyGenerator>());
        generator.Init(template);
        SecretKeyObject secretKeyObject = generator.Generate(BouncyHsm.Core.Services.Bc.HwRandomGenerator.SecureRandom);

        this.UpdateKey(secretKeyObject);
        secretKeyObject.Validate();

        await this.persistentRepository.StoreObject(slotId, secretKeyObject, cancellationToken);

        return new DomainResult<GeneratedSecretId>.Ok(new GeneratedSecretId(secretKeyObject.Id));
    }

    private void UpdatePrivateKey(bool simulateQualifiedArea, PrivateKeyObject privateKeyObject)
    {
        privateKeyObject.CkaLocal = true;
        privateKeyObject.CkaAlwaysSensitive = privateKeyObject.CkaSensitive;

        if (simulateQualifiedArea)
        {
            if (PrivateKeyHelper.ComputeCkaAlwaysAuthenticate(privateKeyObject))
            {
                privateKeyObject.CkaAlwaysAuthenticate = true;
                this.logger.LogInformation("Private key mark as AlwaysAuthenticate.");
            }
            else
            {
                privateKeyObject.CkaAlwaysAuthenticate = false;
            }
        }
    }

    private void UpdatePublicObject(PublicKeyObject publicKeyObject)
    {
        publicKeyObject.CkaLocal = true;
    }

    private void UpdateKeys(SlotEntity slotEntity, PublicKeyObject publicKeyObject, PrivateKeyObject privateKeyObject)
    {
        this.UpdatePublicObject(publicKeyObject);
        this.UpdatePrivateKey(slotEntity.Token.SimulateQualifiedArea, privateKeyObject);

        publicKeyObject.ReComputeAttributes();
        privateKeyObject.ReComputeAttributes();
    }

    private void UpdateKey(SecretKeyObject secretKeyObject)
    {
        secretKeyObject.CkaLocal = true;
        secretKeyObject.ReComputeAttributes();
    }
}
