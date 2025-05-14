using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Core.UseCases.Implementation;
using Scriban;

namespace BouncyHsm.DocGenerator;

public static class Program
{
    public static void Main(string[] args)
    {
        string ouputPath = "Doc.md";
        if (args.Length > 0)
        {
            ouputPath = args[0];
        }

        Template template = Template.Parse(File.ReadAllText("Algorithms.txt"));
        DocModel model = CreateModel();
#pragma warning disable CS8603 // Possible null reference return.
        string content = template.Render(model, new Scriban.Runtime.MemberRenamerDelegate(t => t?.Name));
#pragma warning restore CS8603 // Possible null reference return.

        File.WriteAllText(ouputPath, content, System.Text.Encoding.UTF8);
    }

    private static DocModel CreateModel()
    {
        HsmInfoFacade infoFacade = new HsmInfoFacade();
        DocModel model = new DocModel();

        model.Versions = infoFacade.GetVersions();
        model.Mechanisms = Map(infoFacade.GetAllMechanism().Mechanisms).ToList();
        model.Ec = infoFacade.GetCurves().ToList();
        model.Edwards = infoFacade.GetEdwardsCurves().ToList();
        model.Montgomery = infoFacade.GetMontgomeryCurves().ToList();

        return model;
    }

    private static IEnumerable<MechanismInfo> Map(IEnumerable<MechanismInfoData> enumerable)
    {
        foreach (MechanismInfoData infoData in enumerable)
        {
            ParsedMechanismFlags flags = new ParsedMechanismFlags()
            {
                Derive = infoData.Flags.HasFlag(Core.Services.Contracts.P11.MechanismCkf.CKF_DERIVE),
                Digest = infoData.Flags.HasFlag(Core.Services.Contracts.P11.MechanismCkf.CKF_DIGEST),
                EncryptAndDecrypt = infoData.Flags.HasFlag(Core.Services.Contracts.P11.MechanismCkf.CKF_ENCRYPT),
                Generate = infoData.Flags.HasFlag(Core.Services.Contracts.P11.MechanismCkf.CKF_GENERATE),
                GenerateKeyPair = infoData.Flags.HasFlag(Core.Services.Contracts.P11.MechanismCkf.CKF_GENERATE_KEY_PAIR),
                SignAndVerify = infoData.Flags.HasFlag(Core.Services.Contracts.P11.MechanismCkf.CKF_SIGN),
                SignAndVerifyRecover = infoData.Flags.HasFlag(Core.Services.Contracts.P11.MechanismCkf.CKF_SIGN_RECOVER),
                WrapAndUnwrap = infoData.Flags.HasFlag(Core.Services.Contracts.P11.MechanismCkf.CKF_WRAP),

                IsVersion3_0 = infoData.SpecificationVersion == Core.Services.P11Handlers.Common.Pkcs11SpecVersion.V3_0
            };

            yield return new MechanismInfo(infoData.MechanismType.ToString(),
                infoData.MinKeySize,
               infoData.MaxKeySize,
               flags);
        }
    }
}
