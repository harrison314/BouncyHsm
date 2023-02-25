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
        model.Mechanisms = infoFacade.GetAllMechanism().ToList();
        model.Ec = infoFacade.GetCurves().ToList();

        return model;
    }
}
