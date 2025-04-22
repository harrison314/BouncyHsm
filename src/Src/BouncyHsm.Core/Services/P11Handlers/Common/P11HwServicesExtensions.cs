using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.P11Handlers.SpeedAwaiters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal static class P11HwServicesExtensions
{
    public static async ValueTask<ICryptoApiObject> FindObjectByHandle(this IP11HwServices hwServices,
        IMemorySession memorySession,
        IP11Session session,
        uint objectHandle,
        CancellationToken cancellationToken)
    {
        if (objectHandle == ClockObject.HwHandle)
        {
            return new ClockObject(hwServices.Time);
        }

        Guid? storageObjectId = memorySession.FindObjectHandle(objectHandle);
        if (!storageObjectId.HasValue)
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_OBJECT_HANDLE_INVALID, $"Can not resolve {objectHandle} as object handle.");
        }

        StorageObject? storageObject = session.TryLoadObject(storageObjectId.Value);
        if (storageObject == null)
        {
            storageObject = await hwServices.Persistence.TryLoadObject(session.SlotId, storageObjectId.Value, cancellationToken);
        }

        if (storageObject == null)
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_OBJECT_HANDLE_INVALID, $"Can not load {objectHandle} as object handle.");
        }

        if (storageObject.CkaPrivate && !memorySession.IsUserLogged(session.SlotId))
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_USER_NOT_LOGGED_IN, $"Object handle {objectHandle} with object id {storageObjectId.Value} is private.");
        }

        return storageObject;
    }

    public static async ValueTask<T> FindObjectByHandle<T>(this IP11HwServices hwServices,
        IMemorySession memorySession,
        IP11Session session,
        uint objectHandle,
        CancellationToken cancellationToken)
        where T : ICryptoApiObject
    {
        ICryptoApiObject cryptoApiObject = await FindObjectByHandle(hwServices,
            memorySession,
            session,
            objectHandle,
            cancellationToken);

        if (cryptoApiObject is T value)
        {
            return value;
        }
        else
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_OBJECT_HANDLE_INVALID, $"Object handle {objectHandle} with object id {cryptoApiObject} is not type {typeof(T).Name}.");
        }
    }

    public static async ValueTask<uint> StoreObject(this IP11HwServices hwServices,
       IMemorySession memorySession,
       IP11Session p11Session,
       StorageObject storageObject,
       CancellationToken cancellationToken)
    {
        uint handle;
        if (storageObject.CkaToken)
        {
            await hwServices.Persistence.StoreObject(p11Session.SlotId, storageObject, cancellationToken);
            handle = memorySession.CreateHandle(storageObject);
        }
        else
        {
            p11Session.StoreObject(storageObject);
            handle = memorySession.CreateHandle(storageObject);
        }

        return handle;
    }

    public static async ValueTask<ISpeedAwaiter> CreateSpeedAwaiter(this IP11HwServices hwServices,
        uint slotId,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        SlotEntity? slot = await hwServices.Persistence.GetSlot(slotId, cancellationToken);
        if (slot == null)
        {
            throw new ArgumentException("Slot not found"); //TODO
        }

        return slot.Token.SpeedMode switch
        {
            SpeedMode.WithoutRestriction => new WithoutRestrictionSpeedAwaiter(),
            SpeedMode.Hsm => new HsmSpeedAwaiter(hwServices.Time, loggerFactory.CreateLogger<HsmSpeedAwaiter>()),
            SpeedMode.SmartCard => new SmartCardSpeedAwaiter(hwServices.Time, loggerFactory.CreateLogger<SmartCardSpeedAwaiter>()),
            _ => throw new InvalidProgramException($"Enum value {slot.Token.SpeedMode} is not supported.")
        };
    }
}
