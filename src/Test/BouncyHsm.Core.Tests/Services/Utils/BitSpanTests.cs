using BouncyHsm.Core.Services.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Core.Tests.Services.Utils;

[TestClass]
public sealed class BitSpanTests
{
    [TestMethod]
    [DataRow(true, 8, "FFFFFFFFFFFFFFFF")]
    [DataRow(false, 8, "0000000000000000")]
    [DataRow(true, 1, "FF")]
    [DataRow(false, 1, "00")]
    public void BitSpan_Fill_Success(bool value, int lenghth, string hexOutput)
    {
        byte[] data = new byte[lenghth];
        Random.Shared.NextBytes(data);
        BitSpan bitSpan = new BitSpan(data);
        bitSpan.Fill(value);

        Assert.AreEqual(hexOutput, Convert.ToHexString(data));
    }

    [TestMethod]
    [DataRow(0, 2, "8000")]
    [DataRow(8, 2, "0080")]
    [DataRow(7, 4, "01000000")]
    public void BitSpan_SetBitToTrue_Success(int bitIndex, int lenghth, string hexOutput)
    {
        byte[] data = new byte[lenghth];
        BitSpan bitSpan = new BitSpan(data);
        bitSpan.Fill(false);
        bitSpan[bitIndex] = true;

        Assert.AreEqual(hexOutput, Convert.ToHexString(data));
    }

    [TestMethod]
    [DataRow(0, 2, "7FFF")]
    [DataRow(8, 2, "FF7F")]
    [DataRow(7, 4, "FEFFFFFF")]
    public void BitSpan_SetBitToFalse_Success(int bitIndex, int lenghth, string hexOutput)
    {
        byte[] data = new byte[lenghth];
        BitSpan bitSpan = new BitSpan(data);
        bitSpan.Fill(true);
        bitSpan[bitIndex] = false;

        Assert.AreEqual(hexOutput, Convert.ToHexString(data));
    }

    [TestMethod]
    [DataRow("","")]
    [DataRow("00","00000000")]
    [DataRow("01","00000001")]
    [DataRow("05","00000101")]
    public void BitSpan_ToString_Success(string hex, string output)
    {
        byte[] data = Convert.FromHexString(hex);
        BitSpan bitSpan = new BitSpan(data);

        Assert.AreEqual(output, bitSpan.ToString());
    }
}
