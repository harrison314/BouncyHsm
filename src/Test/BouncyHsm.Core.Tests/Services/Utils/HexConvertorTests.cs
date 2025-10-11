using BouncyHsm.Core.Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Tests.Services.Utils;

[TestClass]
public class HexConvertorTests
{
    [TestMethod]
    [DataRow("0aac1f00")]
    [DataRow("0AAC1F00")]
    [DataRow("0x0AAC1F00")]
    public void GetBytes_Call_Success(string input)
    {
        byte[] excepted = new byte[] { 0x0A, 0xAC, 0x1F, 0x00 };

        byte[] result = HexConvertor.GetBytes(input);
        CollectionAssert.AreEquivalent(excepted, result);
    }

    [TestMethod]
    [DataRow("0aac1f00a")]
    [DataRow("0AAš1F00")]
    [DataRow("0x0\tAC1F00")]
    public void GetBytes_WithError_Throw(string input)
    {
        Assert.Throws<ArgumentException>(() => HexConvertor.GetBytes(input));
    }

    [TestMethod]
    [DataRow("0aac1f00")]
    [DataRow("0AAC1F00")]
    [DataRow("0x0AAC1F00")]
    public void TryGetBytes_Call_Success(string input)
    {
        byte[] excepted = new byte[] { 0x0A, 0xAC, 0x1F, 0x00 };
        Span<byte> result = new byte[12];

        Assert.IsTrue(HexConvertor.TryGetBytes(input, result, out int witeBytes));

        CollectionAssert.AreEquivalent(excepted, result.Slice(0, witeBytes).ToArray());
    }

    [TestMethod]
    public void GetString_LowerCase_Success()
    {
        byte[] input = new byte[] { 0x0A, 0xAC, 0x1F, 0x00 };
        string result = HexConvertor.GetString(input, HexFormat.LowerCase);

        Assert.AreEqual("0aac1f00", result);
    }

    [TestMethod]
    public void GetString_UpperCase_Success()
    {
        byte[] input = new byte[] { 0x0A, 0xAC, 0x1F, 0x00 };
        string result = HexConvertor.GetString(input, HexFormat.UpperCase);

        Assert.AreEqual("0AAC1F00", result);
    }

    [TestMethod]
    public void TryGetString_LowerCase_Success()
    {
        byte[] input = new byte[] { 0x0A, 0xAC, 0x1F, 0x00 };
        Span<char> output = new char[input.Length * 2];
        Assert.IsTrue(HexConvertor.TryGetString(input, HexFormat.LowerCase, output, out int writeChars));

        Assert.AreEqual("0aac1f00", output.ToString());
        Assert.AreEqual(input.Length * 2, writeChars);
    }

    [TestMethod]
    public void TryGetString_UpperCase_Success()
    {
        byte[] input = new byte[] { 0x0A, 0xAC, 0x1F, 0x00 };
        Span<char> output = new char[input.Length * 2];
        Assert.IsTrue(HexConvertor.TryGetString(input, HexFormat.UpperCase, output, out int writeChars));

        Assert.AreEqual("0AAC1F00", output.ToString());
        Assert.AreEqual(input.Length * 2, writeChars);
    }
}
