using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Infrastructure.Common;

public sealed class NullProtectedAuthPathProvider : IProtectedAuthPathProvider
{
    public NullProtectedAuthPathProvider()
    {
        
    }

    public Task<byte[]?> TryLoginProtected(ProtectedAuthPathWindowType windowType, CKU userType, SlotEntity slot, CancellationToken cancellationToken)
    {
        if (slot == null) throw new ArgumentNullException(nameof(slot));

        throw new RpcPkcs11Exception(CKR.CKR_GENERAL_ERROR, "Protected authentication path is not supported.");
    }
}
