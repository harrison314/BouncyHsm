using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Pqc.Crypto.Lms;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public static class HssUtils
{
    public const uint MinLevels = 1;
    public const uint MaxLevels = 8;

    public static LMSigParameters GetLmsParameters(CK_LMS_TYPE lmsType)
    {
        if (!Enum.IsDefined(lmsType))
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT, $"Value {(uint)lmsType:x} is not defined LMS type.");
        }

        return LMSigParameters.GetParametersByID((int)lmsType)
            ?? throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT, $"LMS type {lmsType} is not supported.");
    }

    public static CK_LMS_TYPE GetLmsType(LMSigParameters parameters)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        CK_LMS_TYPE type = (CK_LMS_TYPE)parameters.ID;
        if (!Enum.IsDefined(type))
        {
            throw new InvalidProgramException($"LMS parameters with ID {parameters.ID} are not supported.");
        }

        return type;
    }

    public static LMOtsParameters GetLmOtsParameters(CK_LMOTS_TYPE lmOtsType)
    {
        if (!Enum.IsDefined(lmOtsType))
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT, $"Value {(uint)lmOtsType:x} is not defined LM-OTS type.");
        }

        return LMOtsParameters.GetParametersByID((int)lmOtsType)
            ?? throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT, $"LM-OTS type {lmOtsType} is not supported.");
    }

    public static CK_LMOTS_TYPE GetLmOtsType(LMOtsParameters parameters)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        CK_LMOTS_TYPE type = (CK_LMOTS_TYPE)parameters.ID;
        if (!Enum.IsDefined(type))
        {
            throw new InvalidProgramException($"LM-OTS parameters with ID {parameters.ID} are not supported.");
        }

        return type;
    }

    public static LmsParameters[] CreateParameters(uint levels, uint[] lmsTypes, uint[] lmotsTypes)
    {
        if (lmsTypes == null) throw new ArgumentNullException(nameof(lmsTypes));
        if (lmotsTypes == null) throw new ArgumentNullException(nameof(lmotsTypes));

        if (levels < MinLevels || levels > MaxLevels)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT, $"CKA_HSS_LEVELS must be in range {MinLevels}..{MaxLevels}, but is {levels}.");
        }

        if (lmsTypes.Length != levels || lmotsTypes.Length != levels)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
                $"Count of CKA_HSS_LMS_TYPES ({lmsTypes.Length}) and CKA_HSS_LMOTS_TYPES ({lmotsTypes.Length}) must be equal to CKA_HSS_LEVELS ({levels}).");
        }

        LmsParameters[] result = new LmsParameters[levels];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = new LmsParameters(
                GetLmsParameters((CK_LMS_TYPE)lmsTypes[i]),
                GetLmOtsParameters((CK_LMOTS_TYPE)lmotsTypes[i]));

            // BouncyCastle derives the seed of a child LMS tree from the parent's LM-OTS output (n bytes),
            // so a child tree with a larger hash output (m) cannot be generated ("root seed is less than 32").
            if (i > 0 && result[i].LMSigParameters.M > result[i - 1].LMOtsParameters.N)
            {
                throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
                    $"HSS level {i} uses LMS type {(CK_LMS_TYPE)lmsTypes[i]} with m={result[i].LMSigParameters.M}, " +
                    $"which is larger than n={result[i - 1].LMOtsParameters.N} of the LM-OTS type {(CK_LMOTS_TYPE)lmotsTypes[i - 1]} on level {i - 1}. This combination is not supported.");
            }
        }

        return result;
    }

    public static uint ToKeysRemaining(long usagesRemaining)
    {
        if (usagesRemaining < 0)
        {
            return 0;
        }

        return usagesRemaining > uint.MaxValue ? uint.MaxValue : (uint)usagesRemaining;
    }

    /// <summary>
    /// Exact HSS signature length (RFC 8554 section 6.2): u32 Nspk, then for each level an LMS signature
    /// (u32 q || LM-OTS signature || u32 type || h*m path), plus the LMS public key of every lower level.
    /// </summary>
    public static uint GetSignatureLength(uint[] lmsTypes, uint[] lmotsTypes)
    {
        if (lmsTypes.Length == 0 || lmsTypes.Length != lmotsTypes.Length)
        {
            throw new ArgumentException("LMS and LM-OTS type arrays must be non-empty and of the same length.");
        }

        uint length = 4;
        for (int i = 0; i < lmsTypes.Length; i++)
        {
            LMSigParameters lms = GetLmsParameters((CK_LMS_TYPE)lmsTypes[i]);
            LMOtsParameters ots = GetLmOtsParameters((CK_LMOTS_TYPE)lmotsTypes[i]);

            uint otsSignatureLength = (uint)(4 + ots.N + ots.P * ots.N);
            length += (uint)(4 + otsSignatureLength + 4 + lms.H * lms.M);

            if (i > 0)
            {
                // Signed public key of the lower level: u32 lms type || u32 ots type || I (16) || T1 (m)
                length += (uint)(4 + 4 + 16 + lms.M);
            }
        }

        return length;
    }

    public static string GetParametersName(CK_LMS_TYPE lmsType, CK_LMOTS_TYPE lmOtsType, uint levels)
    {
        return $"HSS L={levels} {lmsType}/{lmOtsType}";
    }

    public static List<string> GetSupportedKeys()
    {
        List<string> result = new List<string>();
        foreach (CK_LMS_TYPE type in Enum.GetValues<CK_LMS_TYPE>())
        {
            result.Add(type.ToString());
        }

        foreach (CK_LMOTS_TYPE type in Enum.GetValues<CK_LMOTS_TYPE>())
        {
            result.Add(type.ToString());
        }

        return result;
    }
}
