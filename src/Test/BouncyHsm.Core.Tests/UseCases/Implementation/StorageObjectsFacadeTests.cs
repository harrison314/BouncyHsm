using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Core.UseCases.Implementation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BouncyHsm.Core.Tests.UseCases.Implementation;

[TestClass]
public class StorageObjectsFacadeTests
{
    [TestMethod]
    public async Task GetStorageObjects_Call_Success()
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.FindObjects(12U, It.IsNotNull<FindObjectSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StorageObject>()
            {
                new DataObject()
                {
                    CkaLabel = "data1",
                    CkaApplication = "app1"
                },
                new X509CertificateObject()
                {
                    CkaId = new byte[]{0x45,0x55},
                    CkaLabel = "certificate1"
                }
            })
            .Verifiable();

        StorageObjectsFacade storageObjectFacade = new StorageObjectsFacade(repository.Object, new NullLogger<StorageObjectsFacade>());

        DomainResult<StorageObjectsList> domainResult = await storageObjectFacade.GetStorageObjects(12U, 0, 10, default);
        StorageObjectsList result = domainResult.AssertOkValue();

        Assert.AreEqual(2, result.TotalCount);
        Assert.AreEqual("data1", result.Objects[0].CkLabel);
        Assert.AreEqual("certificate1", result.Objects[1].CkLabel);
        Assert.IsNotNull(result.Objects[1].CkIdHex);

        repository.VerifyAll();
    }

    [TestMethod]
    public async Task GetStorageObject_Call_Success()
    {
        Guid objectId = Guid.NewGuid();
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.TryLoadObject(12U, objectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataObject() { Id = Guid.NewGuid(), CkaLabel = "label", CkaValue = new byte[32] })
            .Verifiable();

        StorageObjectsFacade storageObjectFacade = new StorageObjectsFacade(repository.Object, new NullLogger<StorageObjectsFacade>());
        DomainResult<StorageObjectDetail> domainResult = await storageObjectFacade.GetStorageObject(12U, objectId, default);
        StorageObjectDetail result = domainResult.AssertOkValue();

        Assert.IsNotNull(result.Description);
        Assert.IsNotNull(result.Attributes);
        Assert.IsNotNull(result.Attributes[0].ValueHex);

        repository.VerifyAll();
    }

    [TestMethod]
    public async Task GetStorageObject_Call_NotFound()
    {
        Guid objectId = Guid.NewGuid();

        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.TryLoadObject(12U, objectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as StorageObject)
            .Verifiable();

        StorageObjectsFacade storageObjectFacade = new StorageObjectsFacade(repository.Object, new NullLogger<StorageObjectsFacade>());
        DomainResult<StorageObjectDetail> domainResult = await storageObjectFacade.GetStorageObject(12U, objectId, default);

        Assert.IsInstanceOfType(domainResult, typeof(DomainResult<StorageObjectDetail>.NotFound));

        repository.VerifyAll();
    }


    [TestMethod]
    public async Task DeleteStorageObject_Call_Success()
    {
        Guid objectId = Guid.NewGuid();
        StorageObject deletedObject = new DataObject()
        {
            CkaLabel = "label",
            CkaValue = new byte[32]
        };

        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.TryLoadObject(12U, objectId, It.IsAny<CancellationToken>()))
           .ReturnsAsync(deletedObject)
           .Verifiable();

        repository.Setup(t => t.DestroyObject(12U, deletedObject, It.IsAny<CancellationToken>()))
            .Returns(new ValueTask())
            .Verifiable();

        StorageObjectsFacade storageObjectFacade = new StorageObjectsFacade(repository.Object, new NullLogger<StorageObjectsFacade>());
        VoidDomainResult domainResult = await storageObjectFacade.DeleteStorageObject(12U, objectId, default);

        domainResult.AssertOk();

        repository.VerifyAll();
    }

    [TestMethod]
    public async Task Download_Call_Success()
    {
        Guid objectId = Guid.NewGuid();
        StorageObject deletedObject = new DataObject()
        {
            CkaLabel = "label",
            CkaValue = new byte[32]
        };

        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.TryLoadObject(12U, objectId, It.IsAny<CancellationToken>()))
           .ReturnsAsync(deletedObject)
           .Verifiable();

        StorageObjectsFacade storageObjectFacade = new StorageObjectsFacade(repository.Object, new NullLogger<StorageObjectsFacade>());
        DomainResult<ObjectContent> domainResult = await storageObjectFacade.Download(12U, objectId, default);
        ObjectContent result = domainResult.AssertOkValue();

        Assert.That.IsNotNullOrEmpty(result.FileName);
        Assert.IsNotNull(result.Content);

        repository.VerifyAll();
    }

    [TestMethod]
    [DataRow(CKA.CKA_VALUE)]
    [DataRow(CKA.CKA_LABEL)]
    [DataRow(CKA.CKA_APPLICATION)]
    [DataRow(CKA.CKA_DESTROYABLE)]
    public async Task GetObjectAttribute_Call_Success(CKA attrType)
    {
        Guid objectId = Guid.NewGuid();
        StorageObject deletedObject = new DataObject()
        {
            Id = objectId,
            CkaLabel = "label",
            CkaValue = new byte[32],
            CkaApplication = "app1",
            CkaDestroyable = true,
        };

        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.TryLoadObject(12U, objectId, It.IsAny<CancellationToken>()))
           .ReturnsAsync(deletedObject)
           .Verifiable();

        StorageObjectsFacade storageObjectFacade = new StorageObjectsFacade(repository.Object, new NullLogger<StorageObjectsFacade>());
        DomainResult<HighLevelAttributeValue> domainResult = await storageObjectFacade.GetObjectAttribute(12U, objectId, attrType.ToString(), default);
        HighLevelAttributeValue result = domainResult.AssertOkValue();

        Assert.IsNotNull(result);

        repository.VerifyAll();
    }

    [TestMethod]
    public async Task SetObjectAttribute_Call_Success()
    {
        Guid objectId = Guid.NewGuid();
        StorageObject deletedObject = new DataObject()
        {
            Id = objectId,
            CkaLabel = "label",
            CkaValue = new byte[32],
            CkaModifiable = false
        };

        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.TryLoadObject(12U, objectId, It.IsAny<CancellationToken>()))
           .ReturnsAsync(deletedObject)
           .Verifiable();

        repository.Setup(t => t.UpdateObject(12U, It.IsNotNull<StorageObject>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask)
            .Verifiable();

        StorageObjectsFacade storageObjectFacade = new StorageObjectsFacade(repository.Object, new NullLogger<StorageObjectsFacade>());

        HighLevelAttributeValue value = new HighLevelAttributeValue()
        {
            TypeTag = AttrTypeTag.CkBool,
            ValueAsBool = true
        };

        VoidDomainResult domainResult = await storageObjectFacade.SetObjectAttribute(12U, objectId, CKA.CKA_MODIFIABLE.ToString(), value, default);
        domainResult.AssertOk();

        repository.VerifyAll();
    }
}