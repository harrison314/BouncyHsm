using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.P11Handlers.SpeedAwaiters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Tests.Services.P11Handlers.SpeedAwaiters;

[TestClass]
public class GenerateKeyVisitorTests
{
    private readonly double[] multiplicator = new double[] { 23.2, 36.0, 36.0 };

    [TestMethod]
    public void Accept_RSA2048_Success()
    {
        RsaPrivateKeyObject rsaPrivateKeyObject = new RsaPrivateKeyObject(Core.Services.Contracts.P11.CKM.CKM_RSA_PKCS_KEY_PAIR_GEN)
        {
            CkaModulus = new byte[2048 / 8]
        };

        GenerateKeyVisitor generateKeyVisitor = new GenerateKeyVisitor(this.multiplicator);

        TimeSpan result = rsaPrivateKeyObject.Accept(generateKeyVisitor);

        this.AssertTimeSpan(TimeSpan.FromSeconds(30.0), result);
    }

    [TestMethod]
    public void Accept_RSA4096_Success()
    {
        RsaPrivateKeyObject rsaPrivateKeyObject = new RsaPrivateKeyObject(Core.Services.Contracts.P11.CKM.CKM_RSA_PKCS_KEY_PAIR_GEN)
        {
            CkaModulus = new byte[4096 / 8]
        };

        GenerateKeyVisitor generateKeyVisitor = new GenerateKeyVisitor(this.multiplicator);

        TimeSpan result = rsaPrivateKeyObject.Accept(generateKeyVisitor);

        this.AssertTimeSpan(TimeSpan.FromSeconds(74.0), result);
    }

    private void AssertTimeSpan(TimeSpan excepted, TimeSpan value)
    {
        TimeSpan diff = excepted / 10;
        if (value < (excepted - diff) || value > (excepted + diff))
        {
            Assert.Fail($"Value is out of range from {excepted - diff} to {excepted + diff}. Actual: {value}");
        }
    }
}
