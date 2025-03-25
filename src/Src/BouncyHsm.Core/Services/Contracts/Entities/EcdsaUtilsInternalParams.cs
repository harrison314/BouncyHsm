using Dunet;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X9;

namespace BouncyHsm.Core.Services.Contracts.Entities;

[Union]
internal partial record EcdsaUtilsInternalParams
{
    partial record NamedCurve(DerObjectIdentifier Oid);

    partial record ExplicitParams(X9ECParameters EcParameters);
}