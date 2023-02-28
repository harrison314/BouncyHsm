namespace BouncyHsm.Core.UseCases.Contracts;

[Dunet.Union]
public partial record SubjectName
{
    partial record Text(string X509NameText);
    partial record OidValuePairs(List<SubjectNameEntry> Pairs);
}
