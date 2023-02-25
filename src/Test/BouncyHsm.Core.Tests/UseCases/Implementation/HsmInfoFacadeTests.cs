using BouncyHsm.Core.UseCases.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Tests.UseCases.Implementation;

[TestClass]
public class HsmInfoFacadeTests
{
    [TestMethod]
    public void GetVersions_Call_Success()
    {
        HsmInfoFacade hsmInfoFacade = new HsmInfoFacade();
        Core.UseCases.Contracts.BouncyHsmVersion versions = hsmInfoFacade.GetVersions();

        Assert.IsNotNull(versions.Version);
        Assert.IsNotNull(versions.BouncyCastleVersion);
        Assert.IsNotNull(versions.Commit);
        Assert.IsNotNull(versions.P11Version);
    }

    [TestMethod]
    public void GetAllMechanism_Call_Success()
    {
        HsmInfoFacade hsmInfoFacade = new HsmInfoFacade();
        IEnumerable<Core.UseCases.Contracts.MechanismInfoData> mechanisms = hsmInfoFacade.GetAllMechanism();

        Assert.IsNotNull(mechanisms.ToList());
    }

    [TestMethod]
    public void GetCurves_Call_Success()
    {
        HsmInfoFacade hsmInfoFacade = new HsmInfoFacade();
        IEnumerable<Services.Contracts.Entities.SupportedNameCurve> curves = hsmInfoFacade.GetCurves();

        Assert.IsNotNull(curves.ToList());
    }
}
