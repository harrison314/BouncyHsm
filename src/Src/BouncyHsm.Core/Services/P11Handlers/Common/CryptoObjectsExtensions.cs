using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal static class CryptoObjectsExtensions
{
    public static void CheckAllowedMechanism(this KeyObject keyObject, CKM mechanism, ILogger logger)
    {
        if (!keyObject.MechanismIsAllowed(mechanism))
        {
            logger.LogError("Object with id {ObjectId} does not have the {Mechanism} mechanism enabled in CKA_ALLOWED_MECHANISMS.",
                keyObject.Id,
                mechanism);
            throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                $"The mechanism {mechanism} is not allowed in CKA_ALLOWED_MECHANISMS on key object.");
        }
    }
}
