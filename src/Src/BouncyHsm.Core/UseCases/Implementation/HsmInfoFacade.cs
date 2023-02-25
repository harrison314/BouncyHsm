using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.UseCases.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.UseCases.Implementation;

public class HsmInfoFacade : IHsmInfoFacade
{
    public HsmInfoFacade()
    {

    }

    public IEnumerable<SupportedNameCurve> GetCurves()
    {
        return EcdsaUtils.GetCurveNames();
    }

    public BouncyHsmVersion GetVersions()
    {
        AssemblyMetadataAttribute? commitHashAttribute = this.GetType().Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .SingleOrDefault(t => t.Key == "CommitHash");

        return new BouncyHsmVersion(
            this.GetType().Assembly.GetName().Version!.ToString(),
            typeof(Org.BouncyCastle.Asn1.DerObjectIdentifier).Assembly.GetName().Version!.ToString(),
            "2.40",
            commitHashAttribute?.Value ?? "-");
    }

    public IEnumerable<MechanismInfoData> GetAllMechanism()
    {
        uint[] mechanisms = MechanismUtils.GetMechanismAsUintArray();

        for (int i = 0; i < mechanisms.Length; i++)
        {
            MechanismUtils.TryGetMechanismInfo((CKM)mechanisms[i], out MechanismInfo mechanismInfo);
            yield return new MechanismInfoData((CKM)mechanisms[i],
                mechanismInfo.MinKeySize,
                mechanismInfo.MaxKeySize,
                mechanismInfo.Flags);
        }
    }
}
