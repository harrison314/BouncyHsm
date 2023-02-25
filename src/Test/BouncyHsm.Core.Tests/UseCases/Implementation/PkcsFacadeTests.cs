using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Core.UseCases.Implementation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BouncyHsm.Core.Tests.UseCases.Implementation;

[TestClass]
public class PkcsFacadeTests
{
    [DataTestMethod]
    [DataRow(P12ImportMode.Local, true, 0)]
    [DataRow(P12ImportMode.Local, false, 0)]
    [DataRow(P12ImportMode.Imported, false, 0)]
    [DataRow(P12ImportMode.LocalInQualifiedArea, false, 0)]
    [DataRow(P12ImportMode.Imported, true, 1)]
    public async Task ImportP12_Call_Success(P12ImportMode mode, bool withChain, int certId)
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.StoreObject(12U, It.Is<StorageObject>(q => q is X509CertificateObject || q is PrivateKeyObject || q is PublicKeyObject), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask())
            .Verifiable();

        repository.Setup(t => t.GetSlot(12U, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SlotEntity()
            {
                Description = "description",
                Id = Guid.NewGuid(),
                IsHwDevice = true,
                SlotId = 12,
                Token = new TokenInfo()
                {
                    IsSoPinLocked = false,
                    IsUserPinLocked = false,
                    Label = "Label",
                    SerialNumber = "000011",
                    SimulateHwMechanism = true,
                    SimulateHwRng = true,
                    SimulateQualifiedArea = true
                }
            })
            .Verifiable();

        PkcsFacade pkcsFacade = new PkcsFacade(repository.Object, new NullLogger<PkcsFacade>());

        ImportP12Request request = new ImportP12Request()
        {
            SlotId = 12U,
            CkaId = new byte[8],
            CkaLabel = "Test",
            ImportChain = withChain,
            ImportMode = mode,
            Password = "Passw0rd",
            Pkcs12Content = this.GetP12Content(certId)
        };

        DomainResult<Guid> domainResult = await pkcsFacade.ImportP12(request, default);
        _ = domainResult.AssertOkValue();

        repository.VerifyAll();
    }

    private byte[] GetP12Content(int id)
    {
        string[] contents = new string[]
        {
            "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"
        };

        return Convert.FromBase64String(contents[id]);
    }
}