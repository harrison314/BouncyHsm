using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Anssi;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.GM;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.Entities;

internal static class EcdsaUtils
{
    private static HashSet<DerObjectIdentifier>? enabledCurves = null;
    public static byte[] EncodeP11EcPoint(Org.BouncyCastle.Math.EC.ECPoint q)
    {
        System.Diagnostics.Debug.Assert(q != null);

        Org.BouncyCastle.Asn1.DerOctetString octets = new DerOctetString(q.GetEncoded(false));
        return octets.GetDerEncoded();
    }

    public static Org.BouncyCastle.Math.EC.ECPoint DecodeP11EcPoint(Org.BouncyCastle.Asn1.X9.X9ECParameters ecParameters, byte[] ecPoint)
    {
        System.Diagnostics.Debug.Assert(ecParameters != null);
        System.Diagnostics.Debug.Assert(ecPoint != null);

        Asn1OctetString octetString = DerOctetString.GetInstance(ecPoint);
        return ecParameters.Curve.DecodePoint(octetString.GetOctets());
    }

    public static X9ECParameters ParseEcParams(byte[] ecParams)
    {
        EcdsaUtilsInternalParams internalParams = ParseEcParamsInternal(ecParams);
        CheckIsSupported(internalParams);

        return internalParams.Match(ecParams => ecParams.Parameters,
            namedCurve => ECNamedCurveTable.GetByOid(namedCurve.Oid),
            implicitCa => throw new System.Diagnostics.UnreachableException());
    }

    public static string ParseEcParamsAsName(byte[] ecParams)
    {
        EcdsaUtilsInternalParams internalParams = ParseEcParamsInternal(ecParams);
        CheckIsSupported(internalParams);

        return internalParams.Match<string>(
            ecParams => $"Explicit-EC_PARAMS-{ecParams.Parameters.Curve.FieldSize}b",
            namedCurve => ECNamedCurveTable.GetName(namedCurve.Oid),
            implicitlyCA => "implicitlyCA");
    }

    public static DerObjectIdentifier GetEcOidFromNameOrOid(string nameOrOid)
    {
        if (string.IsNullOrEmpty(nameOrOid)) throw new ArgumentException("Parameter is null or empty.", nameof(nameOrOid));

        if (char.IsDigit(nameOrOid[0]))
        {
            try
            {
                return new DerObjectIdentifier(nameOrOid);
            }
            catch (FormatException)
            {
                //NOP
            }
        }

        DerObjectIdentifier? identifier = ECNamedCurveTable.GetOid(nameOrOid);
        if (identifier == null)
        {
            throw new ArgumentException("Parameter is not oid or supported named curve.", nameof(nameOrOid));
        }

        return identifier;
    }

    public static IEnumerable<SupportedNameCurve> GetCurveNames()
    {
        if (enabledCurves == null)
        {
            return GetCurveNamesInternal();
        }
        else
        {
            return GetCurveNamesInternal()
                .Where(t => enabledCurves.Contains(new DerObjectIdentifier(t.Oid)));
        }
    }

    public static void SetEnabledCurves(IEnumerable<string> enabledCurveOidOrNames)
    {
        HashSet<DerObjectIdentifier> enabled = new HashSet<DerObjectIdentifier>();
        foreach (string curve in enabledCurveOidOrNames)
        {
            DerObjectIdentifier curveOid = GetEcOidFromNameOrOid(curve);

            if (ECNamedCurveTable.GetByOidLazy(curveOid) == null)
            {
                throw new ArgumentException($"Curve {curve} is not supported.", nameof(enabledCurveOidOrNames));
            }

            enabled.Add(curveOid);
        }

        enabledCurves = enabled;
    }

    public static void ResetEnabledCurves()
    {
        enabledCurves = null;
    }

    public static ECKeyGenerationParameters ParseEcParamsToECKeyGenerationParameters(byte[] ecParams, SecureRandom secureRandom)
    {
        EcdsaUtilsInternalParams internalParams = ParseEcParamsInternal(ecParams);
        CheckIsSupported(internalParams);

        return internalParams.Match<ECKeyGenerationParameters>(
            ecParams => new ECKeyGenerationParameters(new ECDomainParameters(ecParams.Parameters), secureRandom),
            namedCurve => new ECKeyGenerationParameters(namedCurve.Oid, secureRandom),
            implicitlyCA => throw new System.Diagnostics.UnreachableException());
    }

    public static Asn1Object ParseEcParamsToAsn1Object(byte[] ecParams)
    {
        EcdsaUtilsInternalParams internalParams = ParseEcParamsInternal(ecParams);
        CheckIsSupported(internalParams);

        return internalParams.Match<Asn1Object>(
            ecParams => ecParams.Parameters.ToAsn1Object(),
            namedCurve => namedCurve.Oid,
            implicitlyCA => throw new System.Diagnostics.UnreachableException());
    }

    private static IEnumerable<SupportedNameCurve> GetCurveNamesInternal()
    {
        foreach (string name in X962NamedCurves.Names)
        {
            yield return new SupportedNameCurve("X962", name, X962NamedCurves.GetOid(name).Id);
        }

        foreach (string name in SecNamedCurves.Names)
        {
            yield return new SupportedNameCurve("SAC", name, SecNamedCurves.GetOid(name).Id);
        }

        foreach (string name in NistNamedCurves.Names)
        {
            yield return new SupportedNameCurve("NIST", name, NistNamedCurves.GetOid(name).Id);
        }

        foreach (string name in TeleTrusTNamedCurves.Names)
        {
            yield return new SupportedNameCurve("TeleTrusT", name, TeleTrusTNamedCurves.GetOid(name).Id);
        }

        foreach (string name in AnssiNamedCurves.Names)
        {
            yield return new SupportedNameCurve("Ancii", name, AnssiNamedCurves.GetOid(name).Id);
        }

        foreach (string name in ECGost3410NamedCurves.Names)
        {
            yield return new SupportedNameCurve("ECGost3410", name, ECGost3410NamedCurves.GetOid(name).Id);
        }

        foreach (string name in GMNamedCurves.Names)
        {
            yield return new SupportedNameCurve("GMN", name, GMNamedCurves.GetOid(name).Id);
        }
    }

    private static EcdsaUtilsInternalParams ParseEcParamsInternal(byte[] ecParams)
    {
        try
        {
            Asn1Object asn1Object = Asn1Object.FromByteArray(ecParams);
            if (asn1Object is DerObjectIdentifier curveOid)
            {
                if (enabledCurves != null && !enabledCurves.Contains(curveOid))
                {
                    throw new RpcPkcs11Exception(CKR.CKR_CURVE_NOT_SUPPORTED, $"CKA_EC_PARAMS with name curve {curveOid} is not supported in this profile.");
                }

                return new EcdsaUtilsInternalParams.NamedCurve(curveOid);
            }
            else if (asn1Object is DerSequence derSequence)
            {
                try
                {
                    X9ECParameters parsedParams = X9ECParameters.GetInstance(derSequence);
                    return new EcdsaUtilsInternalParams.EcParameters(parsedParams);
                }
                catch (ArgumentException ex)
                {
                    throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, $"CKA_EC_PARAMS is not X9ECParameters ASN1 structure. CKA_EC_PARAMS is {EscapeBytes(ecParams)}.", ex);
                }
            }
            else if (asn1Object is DerNull)
            {
                return new EcdsaUtilsInternalParams.ImplicitlyCA();
            }
            else
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, $"CKA_EC_PARAMS is not OID. CKA_EC_PARAMS is {EscapeBytes(ecParams)}.");
            }
        }
        catch (RpcPkcs11Exception)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, $"CKA_EC_PARAMS is not OID. CKA_EC_PARAMS is {EscapeBytes(ecParams)}.", ex);
        }
    }

    private static void CheckIsSupported(EcdsaUtilsInternalParams ecParams)
    {
        ecParams.Match(
            static ecParams =>
            {
                // NOP
            },
            static namedCurve =>
            {
                if (ECNamedCurveTable.GetByOidLazy(namedCurve.Oid) == null)
                {
                    throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, $"CKA_EC_PARAMS is not named curve oid ({namedCurve}).");
                }
            },
            static implicitCa =>
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, "CKA_EC_PARAMS is DER NULL - is not allowed in PKCS11.");
            });
    }

    private static string EscapeBytes(byte[] bytes)
    {
        return Convert.ToBase64String(bytes, Base64FormattingOptions.InsertLineBreaks);
    }
}
