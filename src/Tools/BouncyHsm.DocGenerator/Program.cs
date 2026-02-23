using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Core.UseCases.Implementation;
using Scriban;
using System;

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

        string thisLocation = GetAssemblyDirectory();

        Template template = Template.Parse(File.ReadAllText(Path.Combine(thisLocation, "Algorithms.txt")));
        DocModel model = CreateModel();
#pragma warning disable CS8603 // Possible null reference return.
        string content = template.Render(model, new Scriban.Runtime.MemberRenamerDelegate(t => t?.Name));
#pragma warning restore CS8603 // Possible null reference return.

        File.WriteAllText(ouputPath, content, System.Text.Encoding.UTF8);

        template = Template.Parse(File.ReadAllText(Path.Combine(thisLocation, "SupportedFunctions.txt")));
        FunctionsModel functionModel = CreateFunctionsModel();
#pragma warning disable CS8603 // Possible null reference return.
        string functionContent = template.Render(functionModel, new Scriban.Runtime.MemberRenamerDelegate(t => t?.Name));
#pragma warning restore CS8603 // Possible null reference return.

        string functionPath = Path.Combine(Path.GetDirectoryName(ouputPath) ?? ".", "SupportedFunctions.md");
        File.WriteAllText(functionPath, functionContent, System.Text.Encoding.UTF8);
    }

    private static DocModel CreateModel()
    {
        HsmInfoFacade infoFacade = new HsmInfoFacade();
        DocModel model = new DocModel();

        SupportedKeys supportedKeys = infoFacade.GetSupportedKeys();

        model.Versions = infoFacade.GetVersions();
        model.Mechanisms = Map(infoFacade.GetAllMechanism().Mechanisms).ToList();
        model.Ec = supportedKeys.EcCurves.ToList();
        model.Edwards = supportedKeys.EdwardsCurves.ToList();
        model.Montgomery = supportedKeys.MontgomeryCurves.ToList();
        model.MlDsa = supportedKeys.MlDsaKeys.ToList();
        model.SlhDsa = supportedKeys.SlhDsaKeys.ToList();
        model.MlKem = supportedKeys.MlKemKeys.ToList();

        return model;
    }

    private static FunctionsModel CreateFunctionsModel()
    {
        HsmInfoFacade infoFacade = new HsmInfoFacade();
        FunctionsModel model = new FunctionsModel();
        model.Versions = infoFacade.GetVersions();
        model.Functions = infoFacade.GetFunctionsState()
            .Select(t => new FunctionInfo(t.FunctionName,
                t.State == ImplementationState.Supported,
                t.State == ImplementationState.OnlyNative))
            .ToList();

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
                EncryptAndDecrypt = infoData.Flags.HasFlag(Core.Services.Contracts.P11.MechanismCkf.CKF_ENCRYPT)
                   || infoData.Flags.HasFlag(Core.Services.Contracts.P11.MechanismCkf.CKF_VERIFY),
                Generate = infoData.Flags.HasFlag(Core.Services.Contracts.P11.MechanismCkf.CKF_GENERATE),
                GenerateKeyPair = infoData.Flags.HasFlag(Core.Services.Contracts.P11.MechanismCkf.CKF_GENERATE_KEY_PAIR),
                SignAndVerify = infoData.Flags.HasFlag(Core.Services.Contracts.P11.MechanismCkf.CKF_SIGN)
                   || infoData.Flags.HasFlag(Core.Services.Contracts.P11.MechanismCkf.CKF_VERIFY),
                SignAndVerifyRecover = infoData.Flags.HasFlag(Core.Services.Contracts.P11.MechanismCkf.CKF_SIGN_RECOVER)
                  || infoData.Flags.HasFlag(Core.Services.Contracts.P11.MechanismCkf.CKF_VERIFY_RECOVER),
                WrapAndUnwrap = infoData.Flags.HasFlag(Core.Services.Contracts.P11.MechanismCkf.CKF_WRAP)
                  || infoData.Flags.HasFlag(Core.Services.Contracts.P11.MechanismCkf.CKF_UNWRAP),
                EncapsulateAndDecapsulate = infoData.Flags.HasFlag(Core.Services.Contracts.P11.MechanismCkf.CKF_ENCAPSULATE)
                  || infoData.Flags.HasFlag(Core.Services.Contracts.P11.MechanismCkf.CKF_DECAPSULATE),

                IsVersion3_1 = infoData.SpecificationVersion == Core.Services.P11Handlers.Common.Pkcs11SpecVersion.V3_1,
                IsVersion3_2 = infoData.SpecificationVersion == Core.Services.P11Handlers.Common.Pkcs11SpecVersion.V3_2,
            };

            yield return new MechanismInfo(infoData.MechanismType.ToString(),
                infoData.MinKeySize,
               infoData.MaxKeySize,
               flags);
        }
    }

    private static string GetAssemblyDirectory()
    {
        Uri thisLocation = new Uri(typeof(Program).Assembly.Location);
        return Path.GetDirectoryName(thisLocation.LocalPath)!;
    }
}
