using Dunet;
using Org.BouncyCastle.Asn1;

namespace BouncyHsm.Core.Services.Contracts.Entities;

[Union]
internal partial record EdUtilsInternalParams
{
    partial record EcParameters(Org.BouncyCastle.Asn1.X9.X9ECParameters Parameters);

    partial record NamedCurve(DerObjectIdentifier Oid);

    partial record ImplicitlyCA();

    partial record PrintableString(string Name);
}