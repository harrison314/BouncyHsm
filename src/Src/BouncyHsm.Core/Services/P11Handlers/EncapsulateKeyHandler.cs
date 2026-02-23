using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Encapsulators;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Utilities.IO.Pem;
using System.Reflection.Emit;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class EncapsulateKeyHandler : IRpcRequestHandler<EncapsulateKeyRequest, EncapsulateKeyEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<EncapsulateKeyHandler> logger;

    public EncapsulateKeyHandler(IP11HwServices hwServices, ILoggerFactory loggerFactory, ILogger<EncapsulateKeyHandler> logger)
    {
        this.hwServices = hwServices;
        this.loggerFactory = loggerFactory;
        this.logger = logger;
    }

    public async Task<EncapsulateKeyEnvelope> Handle(EncapsulateKeyRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId} mechanism {mechanism}.",
           request.SessionId,
           (CKM)request.Mechanism.MechanismType);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);
        if (!p11Session.IsRwSession)
        {
            throw new RpcPkcs11Exception(CKR.CKR_SESSION_READ_ONLY, "EncapsulateKey requires readwrite session");
        }

        PublicKeyObject publicKey = await this.hwServices.FindObjectByHandle<PublicKeyObject>(memorySession, p11Session, request.PublicKeyHandle, cancellationToken);

        MechanismUtils.CheckMechanism(request.Mechanism, MechanismCkf.CKF_ENCAPSULATE);

        Dictionary<CKA, IAttributeValue> template = AttrTypeUtils.BuildDictionaryTemplate(request.Template);

        this.logger.LogTrace("Entering to CreateEncapsulator with mechanism type {mechanismType}", (CKM)request.Mechanism.MechanismType);
        P11EncapsulatorFactory p11EncapsulatorFactory = new P11EncapsulatorFactory(this.loggerFactory);
        IP11Encapsulator encapsulator = p11EncapsulatorFactory.Create(request.Mechanism, publicKey);

        encapsulator.Init(template);

        if (request.IsCiphertextPtrSet)
        {
            EncapsulationResult encapulationResult = encapsulator.Encapsulate(publicKey, p11Session.SecureRandom);
            if (encapulationResult.EncapsulatedData.Length > request.PulCiphertextLen)
            {
                this.logger.LogWarning("Buffer provided for ciphertext is too small. Needed {needed} bytes, but only {provided} bytes provided.",
                    encapulationResult.EncapsulatedData.Length,
                    request.PulCiphertextLen);

                return new EncapsulateKeyEnvelope()
                {
                    Rv = (uint)CKR.CKR_BUFFER_TOO_SMALL,
                    Data = new EncapsulateKeyData()
                    {
                        PulCiphertextLen = (uint)encapulationResult.EncapsulatedData.Length,
                        Ciphertext = null,
                        IsPhKeySet = false,
                        PhKeyHandle = 0
                    }
                };
            }

            encapulationResult.KeyObject.Validate();
            uint phKeyHandle = await this.hwServices.StoreObject(memorySession, p11Session, encapulationResult.KeyObject, cancellationToken);

            if (this.logger.IsEnabled(LogLevel.Information))
            {
                this.logger.LogInformation("Create new symmetric key using {generator}. Key <Id: {publicKeyId}, CK_ID: {publicKeyCkId}, CK_LABEL: {publicKeyCkLabel}>",
                    encapsulator.ToString(),
                    encapulationResult.KeyObject.Id,
                    Convert.ToHexString(encapulationResult.KeyObject.CkaId),
                    encapulationResult.KeyObject.CkaLabel);
            }

            return new EncapsulateKeyEnvelope()
            {
                Rv = (uint)CKR.CKR_OK,
                Data = new EncapsulateKeyData()
                {
                    PulCiphertextLen = (uint)encapulationResult.EncapsulatedData.Length,
                    Ciphertext = encapulationResult.EncapsulatedData,
                    IsPhKeySet = true,
                    PhKeyHandle = phKeyHandle
                }
            };
        }
        else
        {
            return new EncapsulateKeyEnvelope()
            {
                Rv = (uint)CKR.CKR_OK,
                Data = new EncapsulateKeyData()
                {
                    PulCiphertextLen = encapsulator.GetEncapsulatedDataLength(publicKey),
                    Ciphertext = null,
                    IsPhKeySet = false,
                    PhKeyHandle = 0
                }
            };
        }
    }
}
