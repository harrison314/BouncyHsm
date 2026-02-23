using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto.Parameters;

namespace BouncyHsm.Core.Services.Contracts.Entities;

internal static class SlhDsaUtils
{
    public static SlhDsaParameters GetParametersFromType(CK_SLH_DSA_PARAMETER_SET parameterSet)
    {
        return parameterSet switch
        {
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_128S => SlhDsaParameters.slh_dsa_sha2_128s,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_128S => SlhDsaParameters.slh_dsa_shake_128s,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_128F => SlhDsaParameters.slh_dsa_sha2_128f,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_128F => SlhDsaParameters.slh_dsa_shake_128f,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_192S => SlhDsaParameters.slh_dsa_sha2_192s,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_192S => SlhDsaParameters.slh_dsa_shake_192s,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_192F => SlhDsaParameters.slh_dsa_sha2_192f,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_192F => SlhDsaParameters.slh_dsa_shake_192f,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_256S => SlhDsaParameters.slh_dsa_sha2_256s,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_256S => SlhDsaParameters.slh_dsa_shake_256s,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_256F => SlhDsaParameters.slh_dsa_sha2_256f,
            CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_256F => SlhDsaParameters.slh_dsa_shake_256f,
            _ => throw new InvalidProgramException($"Enum value {parameterSet} is not supported.")
        };
    }

    public static CK_SLH_DSA_PARAMETER_SET GetMlDsaparametersType(SlhDsaParameters parameters)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        if (SlhDsaParameters.slh_dsa_sha2_128s.Name == parameters.Name)
        {
            return CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_128S;
        }
        if (SlhDsaParameters.slh_dsa_shake_128s.Name == parameters.Name)
        {
            return CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_128S;
        }
        if (SlhDsaParameters.slh_dsa_sha2_128f.Name == parameters.Name)
        {
            return CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_128F;
        }
        if (SlhDsaParameters.slh_dsa_shake_128f.Name == parameters.Name)
        {
            return CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_128F;
        }
        if (SlhDsaParameters.slh_dsa_sha2_192s.Name == parameters.Name)
        {
            return CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_192S;
        }
        if (SlhDsaParameters.slh_dsa_shake_192s.Name == parameters.Name)
        {
            return CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_192S;
        }
        if (SlhDsaParameters.slh_dsa_sha2_192f.Name == parameters.Name)
        {
            return CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_192F;
        }
        if (SlhDsaParameters.slh_dsa_shake_192f.Name == parameters.Name)
        {
            return CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_192F;
        }
        if (SlhDsaParameters.slh_dsa_sha2_256s.Name == parameters.Name)
        {
            return CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_256S;
        }
        if (SlhDsaParameters.slh_dsa_shake_256s.Name == parameters.Name)
        {
            return CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_256S;
        }
        if (SlhDsaParameters.slh_dsa_sha2_256f.Name == parameters.Name)
        {
            return CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHA2_256F;
        }
        if (SlhDsaParameters.slh_dsa_shake_256f.Name == parameters.Name)
        {
            return CK_SLH_DSA_PARAMETER_SET.CKP_SLH_DSA_SHAKE_256F;
        }

        throw new ArgumentException($"Unsupported ML DSA parameters '{parameters.Name}'.", nameof(parameters));
    }

    public static string GetParametersName(CK_SLH_DSA_PARAMETER_SET parametersSet)
    {
        System.Diagnostics.Debug.Assert(Enum.IsDefined<CK_SLH_DSA_PARAMETER_SET>(parametersSet));

        //TODO: Use span
        return parametersSet.ToString()[4..].Replace('_', '-');
    }

    public static string GetSignatureAlgorithmName(CK_SLH_DSA_PARAMETER_SET ckp)
    {
        return GetParametersFromType(ckp).Name;
    }

    public static List<string> GetSupportedKeys()
    {
        return Enum.GetNames<CK_SLH_DSA_PARAMETER_SET>().Select(t=>t[4..].Replace('_', '-')).ToList();
    }
}