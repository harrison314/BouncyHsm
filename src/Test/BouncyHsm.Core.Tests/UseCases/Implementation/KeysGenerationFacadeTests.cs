using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Core.UseCases.Implementation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Tests.UseCases.Implementation;

[TestClass]
public class KeysGenerationFacadeTests
{
    [TestMethod]
    public async Task GenerateRsaKeyPair_Call_Success()
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.StoreObject(12U, It.Is<StorageObject>(q => q is PrivateKeyObject || q is PublicKeyObject), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask())
            .Verifiable();

        repository.Setup(t => t.GetSlot(12U, It.IsAny<CancellationToken>()))
            .ReturnsAsync(this.GetClotEnity())
            .Verifiable();

        KeysGenerationFacade pkcsFacade = new KeysGenerationFacade(repository.Object, new NullLoggerFactory(), new NullLogger<KeysGenerationFacade>());

        GenerateRsaKeyPairRequest request = new GenerateRsaKeyPairRequest()
        {
            KeySize = 2048,
            KeyAttributes = new GenerateKeyAttributes()
            {
                CkaId = null,
                CkaLabel = "test1",
                Exportable = false,
                ForDerivation = false,
                ForEncryption = true,
                ForSigning = true,
                ForWrap = false,
                Sensitive = true,
            }
        };

        DomainResult<GeneratedKeyPairIds> result = await pkcsFacade.GenerateRsaKeyPair(12U, request, default);
        Assert.IsTrue(result.MatchOk(_ => true, () => false));
    }

    [TestMethod]
    public async Task GenerateEcKeyPair_Call_Success()
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.StoreObject(12U, It.Is<StorageObject>(q => q is PrivateKeyObject || q is PublicKeyObject), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask())
            .Verifiable();

        repository.Setup(t => t.GetSlot(12U, It.IsAny<CancellationToken>()))
            .ReturnsAsync(this.GetClotEnity())
            .Verifiable();

        KeysGenerationFacade pkcsFacade = new KeysGenerationFacade(repository.Object, new NullLoggerFactory(), new NullLogger<KeysGenerationFacade>());

        GenerateEcKeyPairRequest request = new GenerateEcKeyPairRequest()
        {
            OidOrName = "secp256k1",
            KeyAttributes = new GenerateKeyAttributes()
            {
                CkaId = null,
                CkaLabel = "test1",
                Exportable = false,
                ForDerivation = false,
                ForEncryption = true,
                ForSigning = true,
                ForWrap = false,
                Sensitive = true,
            }
        };

        DomainResult<GeneratedKeyPairIds> result = await pkcsFacade.GenerateEcKeyPair(12U, request, default);
        Assert.IsTrue(result.MatchOk(_ => true, () => false));
    }

    [TestMethod]
    public async Task GenerateEdwardsKeyPair_Call_Success()
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.StoreObject(12U, It.Is<StorageObject>(q => q is PrivateKeyObject || q is PublicKeyObject), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask())
            .Verifiable();

        repository.Setup(t => t.GetSlot(12U, It.IsAny<CancellationToken>()))
            .ReturnsAsync(this.GetClotEnity())
            .Verifiable();

        KeysGenerationFacade pkcsFacade = new KeysGenerationFacade(repository.Object, new NullLoggerFactory(), new NullLogger<KeysGenerationFacade>());

        GenerateEdwardsKeyPairRequest request = new GenerateEdwardsKeyPairRequest()
        {
            OidOrName = "id-Ed25519",
            KeyAttributes = new GenerateKeyAttributes()
            {
                CkaId = null,
                CkaLabel = "test1",
                Exportable = false,
                ForDerivation = false,
                ForEncryption = true,
                ForSigning = true,
                ForWrap = false,
                Sensitive = true,
            }
        };

        DomainResult<GeneratedKeyPairIds> result = await pkcsFacade.GenerateEdwardsKeyPair(12U, request, default);
        Assert.IsTrue(result.MatchOk(_ => true, () => false));
    }

    [TestMethod]
    public async Task GenerateMontgomeryKeyPair_Call_Success()
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.StoreObject(12U, It.Is<StorageObject>(q => q is PrivateKeyObject || q is PublicKeyObject), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask())
            .Verifiable();

        repository.Setup(t => t.GetSlot(12U, It.IsAny<CancellationToken>()))
            .ReturnsAsync(this.GetClotEnity())
            .Verifiable();

        KeysGenerationFacade pkcsFacade = new KeysGenerationFacade(repository.Object, new NullLoggerFactory(), new NullLogger<KeysGenerationFacade>());

        GenerateMontgomeryKeyPairRequest request = new GenerateMontgomeryKeyPairRequest()
        {
            OidOrName = "id-X25519",
            KeyAttributes = new GenerateKeyAttributes()
            {
                CkaId = null,
                CkaLabel = "test1",
                Exportable = false,
                ForDerivation = false,
                ForEncryption = true,
                ForSigning = true,
                ForWrap = false,
                Sensitive = true,
            }
        };

        DomainResult<GeneratedKeyPairIds> result = await pkcsFacade.GenerateMontgomeryKeyPair(12U, request, default);
        Assert.IsTrue(result.MatchOk(_ => true, () => false));
    }

    [TestMethod]
    public async Task GenerateSecretKey_Call_Success()
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.StoreObject(12U, It.Is<StorageObject>(q => q is GenericSecretKeyObject), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask())
            .Verifiable();

        repository.Setup(t => t.GetSlot(12U, It.IsAny<CancellationToken>()))
            .ReturnsAsync(this.GetClotEnity())
            .Verifiable();

        KeysGenerationFacade pkcsFacade = new KeysGenerationFacade(repository.Object, new NullLoggerFactory(), new NullLogger<KeysGenerationFacade>());

        GenerateSecretKeyRequest request = new GenerateSecretKeyRequest()
        {
            Size = 32,
            KeyAttributes = new GenerateKeyAttributes()
            {
                CkaId = null,
                CkaLabel = "test1",
                Exportable = false,
                ForDerivation = false,
                ForEncryption = true,
                ForSigning = true,
                ForWrap = false,
                Sensitive = true,
            }
        };

        DomainResult<GeneratedSecretId> result = await pkcsFacade.GenerateSecretKey(12U, request, default);
        Assert.IsTrue(result.MatchOk(_ => true, () => false));
    }

    [TestMethod]
    public async Task GenerateAesKey_Call_Success()
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.StoreObject(12U, It.Is<StorageObject>(q => q is AesKeyObject), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask())
            .Verifiable();

        repository.Setup(t => t.GetSlot(12U, It.IsAny<CancellationToken>()))
            .ReturnsAsync(this.GetClotEnity())
            .Verifiable();

        KeysGenerationFacade pkcsFacade = new KeysGenerationFacade(repository.Object, new NullLoggerFactory(), new NullLogger<KeysGenerationFacade>());

        GenerateAesKeyRequest request = new GenerateAesKeyRequest()
        {
            Size = 32,
            KeyAttributes = new GenerateKeyAttributes()
            {
                CkaId = null,
                CkaLabel = "test1",
                Exportable = false,
                ForDerivation = false,
                ForEncryption = true,
                ForSigning = true,
                ForWrap = false,
                Sensitive = true,
            }
        };

        DomainResult<GeneratedSecretId> result = await pkcsFacade.GenerateAesKey(12U, request, default);
        Assert.IsTrue(result.MatchOk(_ => true, () => false));
    }

    [TestMethod]
    public async Task GeneratePoly1305Key_Call_Success()
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.StoreObject(12U, It.Is<StorageObject>(q => q is Poly1305KeyObject), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask())
            .Verifiable();

        repository.Setup(t => t.GetSlot(12U, It.IsAny<CancellationToken>()))
            .ReturnsAsync(this.GetClotEnity())
            .Verifiable();

        KeysGenerationFacade pkcsFacade = new KeysGenerationFacade(repository.Object, new NullLoggerFactory(), new NullLogger<KeysGenerationFacade>());

        GeneratePoly1305KeyRequest request = new GeneratePoly1305KeyRequest()
        {
            KeyAttributes = new GenerateKeyAttributes()
            {
                CkaId = null,
                CkaLabel = "test1",
                Exportable = false,
                ForDerivation = false,
                ForEncryption = true,
                ForSigning = true,
                ForWrap = false,
                Sensitive = true,
            }
        };

        DomainResult<GeneratedSecretId> result = await pkcsFacade.GeneratePoly1305Key(12U, request, default);
        Assert.IsTrue(result.MatchOk(_ => true, () => false));
    }

    [TestMethod]
    public async Task GenerateChaCha20Key_Call_Success()
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.StoreObject(12U, It.Is<StorageObject>(q => q is ChaCha20KeyObject), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask())
            .Verifiable();

        repository.Setup(t => t.GetSlot(12U, It.IsAny<CancellationToken>()))
            .ReturnsAsync(this.GetClotEnity())
            .Verifiable();

        KeysGenerationFacade pkcsFacade = new KeysGenerationFacade(repository.Object, new NullLoggerFactory(), new NullLogger<KeysGenerationFacade>());

        GenerateChaCha20KeyRequest request = new GenerateChaCha20KeyRequest()
        {
            KeyAttributes = new GenerateKeyAttributes()
            {
                CkaId = null,
                CkaLabel = "test1",
                Exportable = false,
                ForDerivation = false,
                ForEncryption = true,
                ForSigning = true,
                ForWrap = false,
                Sensitive = true,
            }
        };

        DomainResult<GeneratedSecretId> result = await pkcsFacade.GenerateChaCha20Key(12U, request, default);
        Assert.IsTrue(result.MatchOk(_ => true, () => false));
    }

    [TestMethod]
    public async Task GenerateSalsa20Key_Call_Success()
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.StoreObject(12U, It.Is<StorageObject>(q => q is Salsa20KeyObject), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask())
            .Verifiable();

        repository.Setup(t => t.GetSlot(12U, It.IsAny<CancellationToken>()))
            .ReturnsAsync(this.GetClotEnity())
            .Verifiable();

        KeysGenerationFacade pkcsFacade = new KeysGenerationFacade(repository.Object, new NullLoggerFactory(), new NullLogger<KeysGenerationFacade>());

        GenerateSalsa20KeyRequest request = new GenerateSalsa20KeyRequest()
        {
            KeyAttributes = new GenerateKeyAttributes()
            {
                CkaId = null,
                CkaLabel = "test1",
                Exportable = false,
                ForDerivation = false,
                ForEncryption = true,
                ForSigning = true,
                ForWrap = false,
                Sensitive = true,
            }
        };

        DomainResult<GeneratedSecretId> result = await pkcsFacade.GenerateSalsa20Key(12U, request, default);
        Assert.IsTrue(result.MatchOk(_ => true, () => false));
    }

    [TestMethod]
    public async Task GenerateCamelliaKey_Call_Success()
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.StoreObject(12U, It.Is<StorageObject>(q => q is CamelliaKeyObject), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask())
            .Verifiable();

        repository.Setup(t => t.GetSlot(12U, It.IsAny<CancellationToken>()))
            .ReturnsAsync(this.GetClotEnity())
            .Verifiable();

        KeysGenerationFacade pkcsFacade = new KeysGenerationFacade(repository.Object, new NullLoggerFactory(), new NullLogger<KeysGenerationFacade>());

        GenerateCamelliaKeyRequest request = new GenerateCamelliaKeyRequest()
        {
            Size = 32,
            KeyAttributes = new GenerateKeyAttributes()
            {
                CkaId = null,
                CkaLabel = "test1",
                Exportable = false,
                ForDerivation = false,
                ForEncryption = true,
                ForSigning = true,
                ForWrap = false,
                Sensitive = true,
            }
        };

        DomainResult<GeneratedSecretId> result = await pkcsFacade.GenerateCamelliaKey(12U, request, default);
        Assert.IsTrue(result.MatchOk(_ => true, () => false));
    }

    [TestMethod]
    [DataRow(Core.Services.Contracts.P11.CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_44)]
    [DataRow(Core.Services.Contracts.P11.CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_65)]
    [DataRow(Core.Services.Contracts.P11.CK_ML_DSA_PARAMETER_SET.CKP_ML_DSA_87)]
    public async Task GenerateMLDsaKeyPair_Call_Success(Core.Services.Contracts.P11.CK_ML_DSA_PARAMETER_SET ckp)
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.StoreObject(12U, It.Is<StorageObject>(q => q is PrivateKeyObject || q is PublicKeyObject), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask())
            .Verifiable();

        repository.Setup(t => t.GetSlot(12U, It.IsAny<CancellationToken>()))
            .ReturnsAsync(this.GetClotEnity())
            .Verifiable();

        KeysGenerationFacade pkcsFacade = new KeysGenerationFacade(repository.Object, new NullLoggerFactory(), new NullLogger<KeysGenerationFacade>());

        GenerateMLDsaKeyPairRequest request = new GenerateMLDsaKeyPairRequest()
        {
            MlDsaParameter = ckp,
            KeyAttributes = new GenerateKeyAttributes()
            {
                CkaId = null,
                CkaLabel = "test1",
                Exportable = false,
                ForDerivation = false,
                ForEncryption = true,
                ForSigning = true,
                ForWrap = false,
                Sensitive = true,
            }
        };

        DomainResult<GeneratedKeyPairIds> result = await pkcsFacade.GenerateMLDsaKeyPair(12U, request, default);
        Assert.IsTrue(result.MatchOk(_ => true, () => false));
    }

    [TestMethod]
    [DataRow(Core.Services.Contracts.P11.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_128S)]
    [DataRow(Core.Services.Contracts.P11.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_128S)]
    [DataRow(Core.Services.Contracts.P11.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_128F)]
    [DataRow(Core.Services.Contracts.P11.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_128F)]
    [DataRow(Core.Services.Contracts.P11.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_192S)]
    [DataRow(Core.Services.Contracts.P11.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_192S)]
    [DataRow(Core.Services.Contracts.P11.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_192F)]
    [DataRow(Core.Services.Contracts.P11.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_192F)]
    [DataRow(Core.Services.Contracts.P11.CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_256S)]
    public async Task GenerateSlhDsaKeyPair_Call_Success(Core.Services.Contracts.P11.CK_SLH_DSA_PARAMETER_SET ckp)
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.StoreObject(12U, It.Is<StorageObject>(q => q is PrivateKeyObject || q is PublicKeyObject), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask())
            .Verifiable();

        repository.Setup(t => t.GetSlot(12U, It.IsAny<CancellationToken>()))
            .ReturnsAsync(this.GetClotEnity())
            .Verifiable();

        KeysGenerationFacade pkcsFacade = new KeysGenerationFacade(repository.Object, new NullLoggerFactory(), new NullLogger<KeysGenerationFacade>());

        GenerateSlhDsaKeyPairRequest request = new GenerateSlhDsaKeyPairRequest()
        {
            SlhDsaParameter = ckp,
            KeyAttributes = new GenerateKeyAttributes()
            {
                CkaId = null,
                CkaLabel = "test1",
                Exportable = false,
                ForDerivation = false,
                ForEncryption = true,
                ForSigning = true,
                ForWrap = false,
                Sensitive = true,
            }
        };

        DomainResult<GeneratedKeyPairIds> result = await pkcsFacade.GenerateSlhDsaKeyPair(12U, request, default);
        Assert.IsTrue(result.MatchOk(_ => true, () => false));
    }

    [TestMethod]
    public async Task GenerateHssKeyPair_Call_Success()
    {
        List<StorageObject> stored = new List<StorageObject>();
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.StoreObject(12U, It.Is<StorageObject>(q => q is PrivateKeyObject || q is PublicKeyObject), It.IsAny<CancellationToken>()))
            .Callback<uint, StorageObject, CancellationToken>((_, o, _) => stored.Add(o))
            .Returns(new ValueTask())
            .Verifiable();

        repository.Setup(t => t.GetSlot(12U, It.IsAny<CancellationToken>()))
            .ReturnsAsync(this.GetClotEnity())
            .Verifiable();

        KeysGenerationFacade pkcsFacade = new KeysGenerationFacade(repository.Object, new NullLoggerFactory(), new NullLogger<KeysGenerationFacade>());

        GenerateHssKeyPairRequest request = new GenerateHssKeyPairRequest()
        {
            Levels = 1,
            LmsTypes = new[] { Core.Services.Contracts.P11.CK_LMS_TYPE.LMS_SHA256_M32_H5 },
            LmotsTypes = new[] { Core.Services.Contracts.P11.CK_LMOTS_TYPE.LMOTS_SHA256_N32_W8 },
            KeyAttributes = new GenerateKeyAttributes()
            {
                CkaId = null,
                CkaLabel = "test1",
                Exportable = true,
                ForDerivation = false,
                ForEncryption = false,
                ForSigning = true,
                ForWrap = false,
                Sensitive = false,
            }
        };

        DomainResult<GeneratedKeyPairIds> result = await pkcsFacade.GenerateHssKeyPair(12U, request, default);
        Assert.IsTrue(result.MatchOk(_ => true, () => false));

        HssPrivateKeyObject privateKey = stored.OfType<HssPrivateKeyObject>().Single();
        Assert.IsFalse(privateKey.CkaExtractable);
        Assert.IsTrue(privateKey.CkaSensitive);
        Assert.IsFalse(privateKey.CkaCopyable);
        Assert.AreEqual(1, stored.OfType<HssPublicKeyObject>().Count());
    }

    [TestMethod]
    [DataRow(0U, 0)]
    [DataRow(9U, 0)]
    [DataRow(1U, 1)]
    public async Task GenerateHssKeyPair_InvalidParameters_InvalidInput(uint levels, int badKind)
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.GetSlot(12U, It.IsAny<CancellationToken>()))
            .ReturnsAsync(this.GetClotEnity());

        KeysGenerationFacade pkcsFacade = new KeysGenerationFacade(repository.Object, new NullLoggerFactory(), new NullLogger<KeysGenerationFacade>());

        GenerateHssKeyPairRequest request = new GenerateHssKeyPairRequest()
        {
            Levels = levels,
            LmsTypes = Enumerable.Repeat(badKind == 1 ? (Core.Services.Contracts.P11.CK_LMS_TYPE)0x19 : Core.Services.Contracts.P11.CK_LMS_TYPE.LMS_SHA256_M32_H5, (int)levels).ToArray(),
            LmotsTypes = Enumerable.Repeat(Core.Services.Contracts.P11.CK_LMOTS_TYPE.LMOTS_SHA256_N32_W8, (int)levels).ToArray(),
        };

        DomainResult<GeneratedKeyPairIds> result = await pkcsFacade.GenerateHssKeyPair(12U, request, default);
        Assert.IsInstanceOfType<DomainResult<GeneratedKeyPairIds>.InvalidInput>(result);
        repository.Verify(t => t.StoreObject(It.IsAny<uint>(), It.IsAny<StorageObject>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [TestMethod]
    public async Task GenerateHssKeyPair_CountMismatch_InvalidInput()
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.GetSlot(12U, It.IsAny<CancellationToken>()))
            .ReturnsAsync(this.GetClotEnity())
            .Verifiable();

        KeysGenerationFacade pkcsFacade = new KeysGenerationFacade(repository.Object, new NullLoggerFactory(), new NullLogger<KeysGenerationFacade>());

        GenerateHssKeyPairRequest request = new GenerateHssKeyPairRequest()
        {
            Levels = 2,
            LmsTypes = new[] { Core.Services.Contracts.P11.CK_LMS_TYPE.LMS_SHA256_M32_H5 },
            LmotsTypes = new[] { Core.Services.Contracts.P11.CK_LMOTS_TYPE.LMOTS_SHA256_N32_W8 },
        };

        DomainResult<GeneratedKeyPairIds> result = await pkcsFacade.GenerateHssKeyPair(12U, request, default);
        Assert.IsInstanceOfType<DomainResult<GeneratedKeyPairIds>.InvalidInput>(result);
        repository.Verify(t => t.StoreObject(It.IsAny<uint>(), It.IsAny<StorageObject>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [TestMethod]
    [DataRow(Core.Services.Contracts.P11.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_1024)]
    [DataRow(Core.Services.Contracts.P11.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_768)]
    [DataRow(Core.Services.Contracts.P11.CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_512)]
    public async Task GenerateMLKemKeyPair_Call_Success(Core.Services.Contracts.P11.CK_ML_KEM_PARAMETER_SET ckp)
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.StoreObject(12U, It.Is<StorageObject>(q => q is PrivateKeyObject || q is PublicKeyObject), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask())
            .Verifiable();

        repository.Setup(t => t.GetSlot(12U, It.IsAny<CancellationToken>()))
            .ReturnsAsync(this.GetClotEnity())
            .Verifiable();

        KeysGenerationFacade pkcsFacade = new KeysGenerationFacade(repository.Object, new NullLoggerFactory(), new NullLogger<KeysGenerationFacade>());

        GenerateMLKemKeyPairRequest request = new GenerateMLKemKeyPairRequest()
        {
            MlKemParameter = ckp,
            KeyAttributes = new GenerateKeyAttributes()
            {
                CkaId = null,
                CkaLabel = "test1",
                Exportable = false,
                ForDerivation = false,
                ForEncryption = true,
                ForSigning = true,
                ForWrap = false,
                Sensitive = true,
            }
        };

        DomainResult<GeneratedKeyPairIds> result = await pkcsFacade.GenerateMLKemKeyPair(12U, request, default);
        Assert.IsTrue(result.MatchOk(_ => true, () => false));
    }

    private SlotEntity GetClotEnity()
    {
        return new SlotEntity()
        {
            Description = "d",
            Id = Guid.NewGuid(),
            IsHwDevice = true,
            SlotId = 12U,
            Token = new TokenInfo()
            {
                IsSoPinLocked = false,
                IsUserPinLocked = false,
                Label = "test label",
                SerialNumber = "0000000000001",
                SimulateHwMechanism = true,
                SimulateHwRng = true,
                SimulateQualifiedArea = true,
                SpeedMode = SpeedMode.WithoutRestriction
            }
        };
    }
}