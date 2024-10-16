using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Anssi;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.GM;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Asn1.X9;
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

    public static DerObjectIdentifier ParseEcParamsOid(byte[] ecParams)
    {
        DerObjectIdentifier namedCurve = ParseEcParamsInternal(ecParams);

        if (ECNamedCurveTable.GetByOidLazy(namedCurve) != null)
        {
            return namedCurve;
        }

        throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, $"CKA_EC_PARAMS is not named curve oid ({namedCurve}).");
    }

    public static X9ECParameters ParseEcParams(byte[] ecParams)
    {
        DerObjectIdentifier namedCurve = ParseEcParamsInternal(ecParams);

        X9ECParameters? parsedParams = null;
        parsedParams = ECNamedCurveTable.GetByOid(namedCurve);
        if (parsedParams != null)
        {
            return parsedParams;
        }


        throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, "CKA_EC_PARAMS is not named curve oid.");
    }

    public static string ParseEcParamsAsName(byte[] ecParams)
    {
        DerObjectIdentifier namedCurve = ParseEcParamsInternal(ecParams);
        return ECNamedCurveTable.GetName(namedCurve);
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
                throw new ArgumentException($"Curve {curve} is not supported.",nameof(enabledCurveOidOrNames));
            }

            enabled.Add(curveOid);


        }

        enabledCurves = enabled;
    }

    public static void ResetEnabledCurves()
    {
        enabledCurves = null;
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


    private static DerObjectIdentifier ParseEcParamsInternal(byte[] ecParams)
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

                return curveOid;
            }
            else if (asn1Object is DerNull)
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, "CKA_EC_PARAMS is DER NULL - is not allowed in PKCS11.");
            }
            else
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, "CKA_EC_PARAMS is not OID.");
            }
        }
        catch (RpcPkcs11Exception)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, "CKA_EC_PARAMS is not OID.", ex);
        }
    }
}
