using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts;

public interface IProtectedAuthPathProvider
{
    Task<byte[]?> TryLoginProtected(ProtectedAuthPathWindowType windowType, CKU userType, SlotEntity slot, CancellationToken cancellationToken);
}
