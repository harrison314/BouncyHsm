using BouncyHsm.Core.UseCases.Contracts;
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
        Assert.IsNotNull(versions.P11Versions);
    }

    [TestMethod]
    public void GetAllMechanism_Call_Success()
    {
        HsmInfoFacade hsmInfoFacade = new HsmInfoFacade();
        Core.UseCases.Contracts.MechanismProfile mechanisms = hsmInfoFacade.GetAllMechanism();

        Assert.IsNotNull(mechanisms);
        Assert.IsNotNull(mechanisms.Mechanisms);
    }

    [TestMethod]
    public void GetSupportedKeys_Call_Success()
    {
        HsmInfoFacade hsmInfoFacade = new HsmInfoFacade();
        SupportedKeys keys = hsmInfoFacade.GetSupportedKeys();

        Assert.IsNotNull(keys.EcCurves.ToList());
        Assert.IsNotNull(keys.EdwardsCurves.ToList());
        Assert.IsNotNull(keys.MlDsaKeys.ToList());
        Assert.IsNotNull(keys.MlKemKeys.ToList());
        Assert.IsNotNull(keys.MontgomeryCurves.ToList());
        Assert.IsNotNull(keys.RsaKeys.ToList());
    }

    [TestMethod]
    public void GetFunctionsState_Call_Success()
    {
        HsmInfoFacade hsmInfoFacade = new HsmInfoFacade();
        IReadOnlyList<FunctionImplState> funstions = hsmInfoFacade.GetFunctionsState();

        Assert.IsNotNull(funstions);
    }
}
