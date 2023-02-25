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

    public static IEnumerable<SupportedNameCurve> GetCurveNames()
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
            if (asn1Object is DerObjectIdentifier)
            {
                return (DerObjectIdentifier)asn1Object;
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
