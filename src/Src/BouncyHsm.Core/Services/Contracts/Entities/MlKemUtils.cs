using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.Entities;

internal static class MlKemUtils
{
    public static CK_ML_KEM_PARAMETER_SET GetMlDsaparametersType(MLKemParameters parameters)
    {
        if (parameters.Name == MLKemParameters.ml_kem_512.Name)
        {
            return CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_512;
        }

        if (parameters.Name == MLKemParameters.ml_kem_768.Name)
        {
            return CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_768;
        }

        if (parameters.Name == MLKemParameters.ml_kem_1024.Name)
        {
            return CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_1024;
        }

        throw new ArgumentException($"Unsupported ML-KEM parameters '{parameters.Name}'.", nameof(parameters));
    }

    public static MLKemParameters GetParametersFromType(CK_ML_KEM_PARAMETER_SET ckp)
    {
        return ckp switch
        {
            CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_512 => MLKemParameters.ml_kem_512,
            CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_768 => MLKemParameters.ml_kem_768,
            CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_1024 => MLKemParameters.ml_kem_1024,
            _ => throw new InvalidProgramException($"Unsupported ML-KEM parameters type {ckp}."),
        };
    }

    public static string GetParametersName(CK_ML_KEM_PARAMETER_SET ckp)
    {
        return ckp switch
        {
            CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_512 => "ML-KEM-512",
            CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_768 => "ML-KEM-768",
            CK_ML_KEM_PARAMETER_SET.CKP_ML_KEM_1024 => "ML-KEM-1024",
            _ => throw new InvalidProgramException($"Unsupported ML-KEM parameters type {ckp}."),
        };
    }

    public static List<string> GetSupportedKeys()
    {
        return Enum.GetNames<CK_ML_KEM_PARAMETER_SET>().Select(t => t[4..].Replace('_', '-')).ToList();
    }
}
