using Dunet;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X9;

namespace BouncyHsm.Core.Services.Contracts.Entities;

/// <summary>
/// Parameters ::= CHOICE {
///    ecParameters ECParameters,
///    namedCurve CURVES.&id({ CurveNames}),
///    implicitlyCA NULL
/// }
/// </summary>
[Union]
internal partial record EcdsaUtilsInternalParams
{
    partial record EcParameters(X9ECParameters Parameters);
    partial record NamedCurve(DerObjectIdentifier Oid);
    partial record ImplicitlyCA();
}