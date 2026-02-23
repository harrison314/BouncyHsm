using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.UseCases.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.UseCases.Implementation;

public class HsmInfoFacade : IHsmInfoFacade
{
    public HsmInfoFacade()
    {

    }

    public SupportedKeys GetSupportedKeys()
    {
        List<string> rsaKeys = new List<string>();
        MechanismUtils.TryGetMechanismInfo(CKM.CKM_RSA_PKCS_KEY_PAIR_GEN, out MechanismInfo rsaInfo);
        for (uint i = rsaInfo.MinKeySize; i <= rsaInfo.MaxKeySize; i += 1024U)
        {
            rsaKeys.Add($"RSA-{i}");
        }

        return new SupportedKeys()
        {
            RsaKeys = rsaKeys,
            EcCurves = EcdsaUtils.GetCurveNames().ToList(),
            EdwardsCurves = EdEcUtils.GetCurveNames().ToList(),
            MontgomeryCurves = MontgomeryEcUtils.GetCurveNames().ToList(),
            MlDsaKeys = MlDsaUtils.GetSupportedKeys(),
            SlhDsaKeys = SlhDsaUtils.GetSupportedKeys(),
            MlKemKeys = MlKemUtils.GetSupportedKeys(),
        };
    }

    public BouncyHsmVersion GetVersions()
    {
        AssemblyMetadataAttribute? commitHashAttribute = this.GetType().Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .SingleOrDefault(t => t.Key == "CommitHash");

        return new BouncyHsmVersion(
            this.GetType().Assembly.GetName().Version!.ToString(),
            typeof(Org.BouncyCastle.Asn1.DerObjectIdentifier).Assembly.GetName().Version!.ToString(),
            ["2.40", "3.1", "3.2"],
            commitHashAttribute?.Value ?? "-");
    }

    public Contracts.MechanismProfile GetAllMechanism()
    {
        uint[] mechanisms = MechanismUtils.GetMechanismAsUintArray();

        List<MechanismInfoData> mechanismsData = new List<MechanismInfoData>(mechanisms.Length);
        for (int i = 0; i < mechanisms.Length; i++)
        {
            MechanismUtils.TryGetMechanismInfo((CKM)mechanisms[i], out MechanismInfo mechanismInfo);
            mechanismsData.Add(new MechanismInfoData((CKM)mechanisms[i],
                mechanismInfo.MinKeySize,
                mechanismInfo.MaxKeySize,
                mechanismInfo.Flags,
                mechanismInfo.SpecVersion));
        }

        return new Contracts.MechanismProfile(MechanismUtils.GetProfileName(),
            mechanismsData);
    }

    public IReadOnlyList<FunctionImplState> GetFunctionsState()
    {
        return new List<FunctionImplState>()
        {
            new FunctionImplState("C_Initialize",  ImplementationState.Supported),
            new FunctionImplState("C_Finalize",  ImplementationState.Supported),
            new FunctionImplState("C_GetInfo",  ImplementationState.Supported),
            new FunctionImplState("C_GetFunctionList",  ImplementationState.OnlyNative),
            new FunctionImplState("C_GetSlotList",  ImplementationState.Supported),
            new FunctionImplState("C_GetSlotInfo",  ImplementationState.Supported),
            new FunctionImplState("C_GetTokenInfo",  ImplementationState.Supported),
            new FunctionImplState("C_GetMechanismList",  ImplementationState.Supported),
            new FunctionImplState("C_GetMechanismInfo",  ImplementationState.Supported),
            new FunctionImplState("C_InitToken",  ImplementationState.Supported),
            new FunctionImplState("C_InitPIN",  ImplementationState.Supported),
            new FunctionImplState("C_SetPIN",  ImplementationState.Supported),
            new FunctionImplState("C_OpenSession",  ImplementationState.Supported),
            new FunctionImplState("C_CloseSession",  ImplementationState.Supported),
            new FunctionImplState("C_CloseAllSessions",  ImplementationState.Supported),
            new FunctionImplState("C_GetSessionInfo",  ImplementationState.Supported),
            new FunctionImplState("C_GetOperationState",  ImplementationState.NotSupported),
            new FunctionImplState("C_SetOperationState",  ImplementationState.NotSupported),
            new FunctionImplState("C_Login",  ImplementationState.Supported),
            new FunctionImplState("C_Logout",  ImplementationState.Supported),
            new FunctionImplState("C_CreateObject",  ImplementationState.Supported),
            new FunctionImplState("C_CopyObject",  ImplementationState.Supported),
            new FunctionImplState("C_DestroyObject",  ImplementationState.Supported),
            new FunctionImplState("C_GetObjectSize",  ImplementationState.Supported),
            new FunctionImplState("C_GetAttributeValue",  ImplementationState.Supported),
            new FunctionImplState("C_SetAttributeValue",  ImplementationState.Supported),
            new FunctionImplState("C_FindObjectsInit",  ImplementationState.Supported),
            new FunctionImplState("C_FindObjects",  ImplementationState.Supported),
            new FunctionImplState("C_FindObjectsFinal",  ImplementationState.Supported),
            new FunctionImplState("C_EncryptInit",  ImplementationState.Supported),
            new FunctionImplState("C_Encrypt",  ImplementationState.Supported),
            new FunctionImplState("C_EncryptUpdate",  ImplementationState.Supported),
            new FunctionImplState("C_EncryptFinal",  ImplementationState.Supported),
            new FunctionImplState("C_DecryptInit",  ImplementationState.Supported),
            new FunctionImplState("C_Decrypt",  ImplementationState.Supported),
            new FunctionImplState("C_DecryptUpdate",  ImplementationState.Supported),
            new FunctionImplState("C_DecryptFinal",  ImplementationState.Supported),
            new FunctionImplState("C_DigestInit",  ImplementationState.Supported),
            new FunctionImplState("C_Digest",  ImplementationState.Supported),
            new FunctionImplState("C_DigestUpdate",  ImplementationState.Supported),
            new FunctionImplState("C_DigestKey",  ImplementationState.Supported),
            new FunctionImplState("C_DigestFinal",  ImplementationState.Supported),
            new FunctionImplState("C_SignInit",  ImplementationState.Supported),
            new FunctionImplState("C_Sign",  ImplementationState.Supported),
            new FunctionImplState("C_SignUpdate",  ImplementationState.Supported),
            new FunctionImplState("C_SignFinal",  ImplementationState.Supported),
            new FunctionImplState("C_SignRecoverInit",  ImplementationState.Supported),
            new FunctionImplState("C_SignRecover",  ImplementationState.Supported),
            new FunctionImplState("C_VerifyInit",  ImplementationState.Supported),
            new FunctionImplState("C_Verify",  ImplementationState.Supported),
            new FunctionImplState("C_VerifyUpdate",  ImplementationState.Supported),
            new FunctionImplState("C_VerifyFinal",  ImplementationState.Supported),
            new FunctionImplState("C_VerifyRecoverInit",  ImplementationState.Supported),
            new FunctionImplState("C_VerifyRecover",  ImplementationState.Supported),
            new FunctionImplState("C_DigestEncryptUpdate",  ImplementationState.NotSupported),
            new FunctionImplState("C_DecryptDigestUpdate",  ImplementationState.NotSupported),
            new FunctionImplState("C_SignEncryptUpdate",  ImplementationState.NotSupported),
            new FunctionImplState("C_DecryptVerifyUpdate",  ImplementationState.NotSupported),
            new FunctionImplState("C_GenerateKey",  ImplementationState.Supported),
            new FunctionImplState("C_GenerateKeyPair",  ImplementationState.Supported),
            new FunctionImplState("C_WrapKey",  ImplementationState.Supported),
            new FunctionImplState("C_UnwrapKey",  ImplementationState.Supported),
            new FunctionImplState("C_DeriveKey",  ImplementationState.Supported),
            new FunctionImplState("C_SeedRandom",  ImplementationState.Supported),
            new FunctionImplState("C_GenerateRandom",  ImplementationState.Supported),
            new FunctionImplState("C_GetFunctionStatus",  ImplementationState.OnlyNative),
            new FunctionImplState("C_CancelFunction",  ImplementationState.OnlyNative),
            new FunctionImplState("C_WaitForSlotEvent",  ImplementationState.Supported),
            new FunctionImplState("C_GetInterfaceList",  ImplementationState.OnlyNative),
            new FunctionImplState("C_GetInterface",  ImplementationState.OnlyNative),
            new FunctionImplState("C_LoginUser",  ImplementationState.NotSupported),
            new FunctionImplState("C_SessionCancel",  ImplementationState.Supported),
            new FunctionImplState("C_MessageEncryptInit",  ImplementationState.NotSupported),
            new FunctionImplState("C_EncryptMessage",  ImplementationState.NotSupported),
            new FunctionImplState("C_EncryptMessageBegin",  ImplementationState.NotSupported),
            new FunctionImplState("C_EncryptMessageNext",  ImplementationState.NotSupported),
            new FunctionImplState("C_MessageEncryptFinal",  ImplementationState.NotSupported),
            new FunctionImplState("C_MessageDecryptInit",  ImplementationState.NotSupported),
            new FunctionImplState("C_DecryptMessage",  ImplementationState.NotSupported),
            new FunctionImplState("C_DecryptMessageBegin",  ImplementationState.NotSupported),
            new FunctionImplState("C_DecryptMessageNext",  ImplementationState.NotSupported),
            new FunctionImplState("C_MessageDecryptFinal",  ImplementationState.NotSupported),
            new FunctionImplState("C_MessageSignInit",  ImplementationState.NotSupported),
            new FunctionImplState("C_SignMessage",  ImplementationState.NotSupported),
            new FunctionImplState("C_SignMessageBegin",  ImplementationState.NotSupported),
            new FunctionImplState("C_SignMessageNext",  ImplementationState.NotSupported),
            new FunctionImplState("C_MessageSignFinal",  ImplementationState.NotSupported),
            new FunctionImplState("C_MessageVerifyInit",  ImplementationState.NotSupported),
            new FunctionImplState("C_VerifyMessage",  ImplementationState.NotSupported),
            new FunctionImplState("C_VerifyMessageBegin",  ImplementationState.NotSupported),
            new FunctionImplState("C_VerifyMessageNext",  ImplementationState.NotSupported),
            new FunctionImplState("C_MessageVerifyFinal",  ImplementationState.NotSupported),
            new FunctionImplState("C_EncapsulateKey",  ImplementationState.Supported),
            new FunctionImplState("C_DecapsulateKey",  ImplementationState.Supported),
            new FunctionImplState("C_VerifySignatureInit",  ImplementationState.NotSupported),
            new FunctionImplState("C_VerifySignature",  ImplementationState.NotSupported),
            new FunctionImplState("C_VerifySignatureUpdate",  ImplementationState.NotSupported),
            new FunctionImplState("C_VerifySignatureFinal",  ImplementationState.NotSupported),
            new FunctionImplState("C_GetSessionValidationFlags",  ImplementationState.Supported),
            new FunctionImplState("C_AsyncComplete",  ImplementationState.NotSupported),
            new FunctionImplState("C_AsyncGetID",  ImplementationState.NotSupported),
            new FunctionImplState("C_AsyncJoin",  ImplementationState.NotSupported),
            new FunctionImplState("C_WrapKeyAuthenticated",  ImplementationState.NotSupported),
            new FunctionImplState("C_UnwrapKeyAuthenticated",  ImplementationState.NotSupported),
        };
    }
}
