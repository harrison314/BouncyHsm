using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Pqc.Crypto.Lms;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

/// <summary>
/// HSS signer which reloads the private key under the per-object write lock, signs,
/// and persists the advanced state before the signature is released.
/// </summary>
internal class HssStatefulSigner : IStatefulSigner
{
    private readonly IP11HwServices hwServices;
    private readonly IMemorySession memorySession;
    private readonly IP11Session session;
    private readonly Guid keyId;
    private readonly bool isToken;
    private readonly ILogger logger;

    public uint SignatureSize
    {
        get;
    }

    public HssStatefulSigner(IP11HwServices hwServices, IMemorySession memorySession, IP11Session session, Guid keyId, bool isToken, uint signatureSize, ILogger logger)
    {
        System.Diagnostics.Debug.Assert(hwServices != null);
        System.Diagnostics.Debug.Assert(session != null);
        System.Diagnostics.Debug.Assert(keyId != Guid.Empty);

        this.hwServices = hwServices;
        this.memorySession = memorySession;
        this.session = session;
        this.keyId = keyId;
        this.isToken = isToken;
        this.SignatureSize = signatureSize;
        this.logger = logger;
    }

    public async ValueTask<byte[]> Sign(byte[] data, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Sign with key {KeyId}.", this.keyId);

        using IDisposable objectLock = await ObjectWriteLocks.Acquire(this.keyId, cancellationToken);

        StorageObject? storageObject = this.isToken
            ? await this.hwServices.Persistence.TryLoadObject(this.session.SlotId, this.keyId, cancellationToken)
            : this.memorySession.TryLoadObject(this.session, this.keyId);

        if (storageObject is not HssPrivateKeyObject keyObject)
        {
            this.logger.LogError("HSS private key with id {KeyId} not found.", this.keyId);
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, "HSS private key not found.");
        }

        HssPrivateKeyParameters privateKey = (HssPrivateKeyParameters)keyObject.GetPrivateKey();
        if (privateKey.GetUsagesRemaining() == 0)
        {
            this.logger.LogWarning("HSS private key with id {KeyId} is exhausted.", this.keyId);
            throw new RpcPkcs11Exception(CKR.CKR_KEY_EXHAUSTED, "HSS private key is exhausted.");
        }

        byte[] signature;
        try
        {
            HssSigner signer = new HssSigner();
            signer.Init(true, privateKey);
            signature = signer.GenerateSignature(data);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "HSS signing failed with key {KeyId}.", this.keyId);
            throw new RpcPkcs11Exception(CKR.CKR_GENERAL_ERROR, $"HSS signing failed: {ex.Message}");
        }

        keyObject.SetPrivateKey(privateKey);

        if (this.isToken)
        {
            await this.hwServices.Persistence.UpdateObject(this.session.SlotId, keyObject, cancellationToken);
        }
        else
        {
            this.memorySession.UpdateObject(this.session, keyObject);
        }

        this.logger.LogDebug("HSS key {KeyId} state persisted, keys remaining {KeysRemaining}.", this.keyId, keyObject.CkaHssKeysRemaining);

        return signature;
    }
}
