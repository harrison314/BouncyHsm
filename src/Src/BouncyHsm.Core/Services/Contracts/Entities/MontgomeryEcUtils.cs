using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.EdEC;
using Org.BouncyCastle.Asn1.X9;
using System.Diagnostics;

namespace BouncyHsm.Core.Services.Contracts.Entities;

internal static class MontgomeryEcUtils
{
    private const string NameX25519 = "id-X25519";
    private const string NameX448 = "id-X448";

    public static IEnumerable<SupportedNameCurve> GetCurveNames()
    {
        yield return new SupportedNameCurve("Montgomery", "X25519", NameX25519, EdECObjectIdentifiers.id_X25519.Id);
        yield return new SupportedNameCurve("Montgomery", "X448", NameX448, EdECObjectIdentifiers.id_X448.Id);
    }

    public static DerObjectIdentifier GetOidFromParams(byte[] ecParams)
    {
        EdOrXUtilsInternalParams paramsUinon = ParseEcParamsInternal(ecParams);
        CheckIsSupported(paramsUinon);

        return paramsUinon.Match<DerObjectIdentifier>(static ecParams => throw new UnreachableException(),
            static namedCurveOid => namedCurveOid.Oid,
            static implicitCa => throw new UnreachableException(),
            static namedCurve =>
            {
                return namedCurve.Name switch
                {
                    NameX25519 => EdECObjectIdentifiers.id_X25519,
                    NameX448 => EdECObjectIdentifiers.id_X448,
                    _ => throw new UnreachableException($"{namedCurve.Name} is not supported")
                };
            });
    }

    public static string? ParseEcParamsAsName(byte[] ecParams)
    {
        EdOrXUtilsInternalParams paramsUinon = ParseEcParamsInternal(ecParams);
        CheckIsSupported(paramsUinon);

        return paramsUinon.Match<string>(static ecParams => throw new UnreachableException(),
            static namedCurveOid =>
            {

                if (namedCurveOid.Oid.Equals(EdECObjectIdentifiers.id_X25519))
                {
                    return NameX25519;
                }

                if (namedCurveOid.Oid.Equals(EdECObjectIdentifiers.id_X448))
                {
                    return NameX448;
                }

                throw new UnreachableException();
            },
            static implicitCa => throw new UnreachableException(),
            static namedCurve =>
            {
                return namedCurve.Name;
            });
    }

    public static byte[] CreateEcparam(string oidOrName)
    {
        if (oidOrName == null) throw new ArgumentNullException(nameof(oidOrName));

        //TODO change after Restrictions
        foreach (SupportedNameCurve supportedNameCurve in GetCurveNames())
        {
            if (string.Equals(oidOrName, supportedNameCurve.Oid, StringComparison.Ordinal))
            {
                return new DerObjectIdentifier(supportedNameCurve.Oid).GetEncoded();
            }

            if (string.Equals(oidOrName, supportedNameCurve.NamedCurve, StringComparison.OrdinalIgnoreCase))
            {
                return new DerPrintableString(supportedNameCurve.NamedCurve).GetEncoded();
            }
        }

        throw new ArgumentException("Parameter oidOrName is not valid edwards curve name or OID.", nameof(oidOrName));
    }

    private static EdOrXUtilsInternalParams ParseEcParamsInternal(byte[] ecParams)
    {
        try
        {
            Asn1Object asn1Object = Asn1Object.FromByteArray(ecParams);
            if (asn1Object is DerObjectIdentifier curveOid)
            {
                //TODO: profile functions
                //if (enabledCurves != null && !enabledCurves.Contains(curveOid))
                //{
                //    throw new RpcPkcs11Exception(CKR.CKR_CURVE_NOT_SUPPORTED, $"CKA_EC_PARAMS with name curve {curveOid} is not supported in this profile.");
                //}

                return new EdOrXUtilsInternalParams.NamedCurve(curveOid);
            }
            else if (asn1Object is DerPrintableString printableString)
            {
                //TODO: profile functions
                //if (enabledCurves != null && !enabledCurves.Contains(curveOid))
                //{
                //    throw new RpcPkcs11Exception(CKR.CKR_CURVE_NOT_SUPPORTED, $"CKA_EC_PARAMS with name curve {curveOid} is not supported in this profile.");
                //}

                return new EdOrXUtilsInternalParams.PrintableString(printableString.GetString());
            }
            else if (asn1Object is DerSequence derSequence)
            {
                try
                {
                    X9ECParameters parsedParams = X9ECParameters.GetInstance(derSequence);
                    return new EdOrXUtilsInternalParams.EcParameters(parsedParams);
                }
                catch (ArgumentException ex)
                {
                    throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, $"CKA_EC_PARAMS is not X9ECParameters ASN1 structure. CKA_EC_PARAMS is {EscapeBytes(ecParams)}.", ex);
                }
            }
            else if (asn1Object is DerNull)
            {
                return new EdOrXUtilsInternalParams.ImplicitlyCA();
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

    private static void CheckIsSupported(EdOrXUtilsInternalParams ecParams)
    {
        ecParams.Match(
            static ecParams =>
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, "CKA_EC_PARAMS is ecParams struct - not allowed for edwards curves.");
            },
            static namedCurve =>
            {
                if (!namedCurve.Oid.Equals(EdECObjectIdentifiers.id_X25519)
                        && !namedCurve.Oid.Equals(EdECObjectIdentifiers.id_X448))
                {
                    throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, $"CKA_EC_PARAMS is not named curve oid ({namedCurve}) for edwards curves.");
                }
            },
            static implicitCa =>
            {
                throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, "CKA_EC_PARAMS is DER NULL - is not allowed in PKCS11.");
            },
            static namedCurve =>
            {
                if (namedCurve.Name != NameX25519 && namedCurve.Name != NameX448)
                {
                    throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, $"CKA_EC_PARAMS is not named curve oid ({namedCurve}) for edwards curves.");
                }
            });
    }

    private static string EscapeBytes(byte[] bytes)
    {
        return Convert.ToBase64String(bytes, Base64FormattingOptions.InsertLineBreaks);
    }
}
