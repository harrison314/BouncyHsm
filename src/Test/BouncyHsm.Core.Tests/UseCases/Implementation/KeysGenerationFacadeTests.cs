﻿using BouncyHsm.Core.Services.Contracts;
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