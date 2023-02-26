using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Core.UseCases.Implementation;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BouncyHsm.Core.Tests.UseCases.Implementation;

[TestClass]
public class SlotFacadeTests
{
    [TestMethod]
    public async Task CreateSlot_Call_Success()
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.CreateSlot(It.IsNotNull<SlotEntity>(), It.IsNotNull<TokenPins>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SlotIds(Guid.NewGuid(), 12))
            .Verifiable();

        SlotFacade slotFacade = new SlotFacade(repository.Object, new NullLogger<SlotFacade>());

        DomainResult<CreateSlotResult> result = await slotFacade.CreateSlot(new CreateSlotData()
        {
            Description = "some decription",
            IsHwDevice = true,
            Token = new CreateTokenData()
            {
                Label = "Label41",
                SerialNumber = null,
                SignaturePin = null,
                SimulateHwMechanism = true,
                SimulateHwRng = false,
                SimulateQualifiedArea = false,
                SoPin = "12345678",
                UserPin = "12345"
            }
        },
        default);

        CreateSlotResult value = result.AssertOkValue();
        Assert.AreEqual(value.SlotId, 12U);
        Assert.AreNotEqual(Guid.Empty, value.Id);
        Assert.IsNotNull(value.TokenSerialNumber);

        repository.VerifyAll();
    }

    [TestMethod]
    public async Task GetSlots_Call_Success()
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.GetSlots(It.IsNotNull<GetSlotSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SlotEntity>()
            {
                new SlotEntity()
                {
                    Description = "description",
                    Id = Guid.NewGuid(),
                    IsHwDevice = true,
                    SlotId= 12,
                    Token = new TokenInfo()
                    {
                        IsSoPinLocked = false,
                        IsUserPinLocked = false,
                        Label = "Label",
                        SerialNumber = "000011",
                        SimulateHwMechanism= true,
                        SimulateHwRng = false,
                        SimulateQualifiedArea = false
                    }
                }
            })
            .Verifiable();

        SlotFacade slotFacade = new SlotFacade(repository.Object, new NullLogger<SlotFacade>());

        DomainResult<IReadOnlyList<SlotEntity>> result = await slotFacade.GetAllSlots(default);

        IReadOnlyList<SlotEntity> value = result.AssertOkValue();
        Assert.AreEqual(value[0].SlotId, 12U);
        Assert.IsNotNull(value[0].Token);
        Assert.IsNotNull(value[0].Token?.SerialNumber);
        Assert.IsNotNull(value[0].Token?.Label);
    }

    [TestMethod]
    public async Task DeleteSlot_Call_Success()
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.DeleteSlot(12U, It.IsAny<CancellationToken>()))
            .Returns(new ValueTask())
            .Verifiable();

        SlotFacade slotFacade = new SlotFacade(repository.Object, new NullLogger<SlotFacade>());

        VoidDomainResult domainResult = await slotFacade.DeleteSlot(12U, default);

        domainResult.AssertOk();

        repository.VerifyAll();
    }
}
