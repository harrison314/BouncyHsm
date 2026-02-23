using BouncyHsm.Core.Services.Bc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class AesWrapperSigner : IWrapperSigner
{
    private readonly CKM mechanismType;
    private readonly IMac mac;
    private readonly int? generalParameter;
    private readonly ILogger<AesWrapperSigner> logger;

    public AesWrapperSigner(CKM mechanismType,
        IMac mac,
        int? generalParameter,
        ILogger<AesWrapperSigner> logger)
    {
        this.mechanismType = mechanismType;
        this.mac = mac;
        this.generalParameter = generalParameter;
        this.logger = logger;
    }

    public AuthenticatedSigner IntoSigningSigner(KeyObject keyObject, SecureRandom secureRandom)
    {
        this.logger.LogTrace("Enteri to IntoSigningSigner");

        AesKeyObject secretKey = this.CheckKey(keyObject);
        if (!secretKey.CkaSign)
        {
            this.logger.LogError("Object with id {ObjectId} can not set CKA_SIGN to true.", secretKey.Id);
            throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                "The signature operation is not allowed because objet is not authorized to sign (CKA_SIGN must by true).");
        }

        ISigner signer = this.CreateSigner();
        signer.Init(true, new KeyParameter(secretKey.GetSecret()));

        return new AuthenticatedSigner(signer, false);
    }

    public ISigner IntoValidationSigner(KeyObject keyObject)
    {
        this.logger.LogTrace("Enteri to IntoValidationSigner");

        AesKeyObject secretKey = this.CheckKey(keyObject);
        if (!secretKey.CkaVerify)
        {
            this.logger.LogError("Object with id {ObjectId} can not set CKA_VERIFY to true.", secretKey.Id);
            throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                "The verification signature operation is not allowed because objet is not authorized to verify (CKA_VERIFY must by true).");
        }

        ISigner signer = this.CreateSigner();
        signer.Init(false, new KeyParameter(secretKey.GetSecret()));

        return signer;
    }

    private AesKeyObject CheckKey(KeyObject keyObject)
    {
        if (keyObject is AesKeyObject aesKeyObject)
        {
            return aesKeyObject;
        }

        throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanismType} required  AES key.");
    }

    private ISigner CreateSigner()
    {
        try
        {
            return (this.generalParameter.HasValue)
            ? new MacGeneralSignerAdapter(this.mac, this.generalParameter.Value)
            : new MacSignerAdapter(this.mac);

        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                $"Mechanism param for {this.mechanismType} has invalid length.",
                ex);
        }
    }
}
