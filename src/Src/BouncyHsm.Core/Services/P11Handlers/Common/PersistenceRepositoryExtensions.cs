using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal static class PersistenceRepositoryExtensions
{
    public static async ValueTask<SlotEntity> EnsureSlot(this IPersistentRepository persistentRepository, uint slotId, CancellationToken cancellationToken)
    {
        SlotEntity? slot = await persistentRepository.GetSlot(slotId, cancellationToken);

        if (slot == null)
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_SLOT_ID_INVALID, $"Slot with id {slotId} not found.");
        }

        return slot;
    }
}
