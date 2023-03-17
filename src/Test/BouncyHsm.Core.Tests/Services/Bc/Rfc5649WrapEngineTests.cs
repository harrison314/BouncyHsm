using BouncyHsm.Core.Services.Bc;
using BouncyHsm.Core.Services.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Tests.Services.Bc;

[TestClass]
public class Rfc5649WrapEngineTests
{

    [DataTestMethod]
    [DataRow("aa0c6b94bcc98519a50a064622faf33e", "ef792023", "FD63F38ADED0D798CEF24532E566EB7D")]
    [DataRow("aa0c6b94bcc98519a50a064622faf33e", "295571f47bbc7f08f46a04a01e598794e2", "A8B8E60E56B5259EEE8266AEA64E821E2AC291FB12A60F9F5F3D92E966BCAB48")]
    [DataRow("aa0c6b94bcc98519a50a064622faf33e", "f635f3749a18307d3e2c7909e6105d5884510b66f639125f4dc6214112ebb1ae1de2d4", "1317C6AA70D9B5B40EB5FD33A3B013E22B6B5F6772349B21B5E045A70667AB7E81BA151523C542B9E2EFA37EC85A3A5E")]
    [DataRow("aa0c6b94bcc98519a50a064622faf33e", "73d4292807f5f53469791261b0353741a013811e1e11a0c2949812cc55c6ebb8fc1c5bf3eadfaae9c7326d6c1fa3b38e0c0f78b4333e57bc583868a7eb9a4f2c82f7e3405b3172d7f66818a947b527b2e0b91bcb6fd44f8d782211c91741d49517440b3ec8", "6ADF2AB905950F09ABC2E005E590805D0DC1B3E67A6FAAF4982769CE3223D33FA059A62B3611D605C1EFC140A0E5BF4B97AE5F00F98AB7271360337110F28E01BD87083C5CE20B6AB092DA1214B3380C8BCEFD2C8BF78C92D07F6FC8C714BE0FBF9276FEB70FF6655F834F2F88532CFB")]
    public void WrapData_Aes_Success(string key, string input, string output)
    {
        byte[] keyBytes = HexConvertor.GetBytes(key);
        byte[] inputBytes = HexConvertor.GetBytes(input);
        byte[] outputBytes = HexConvertor.GetBytes(output);

        Rfc5649WrapEngine engine = new Rfc5649WrapEngine(AesUtilities.CreateEngine());
        engine.Init(true, new KeyParameter(keyBytes));

        byte[] result = engine.Wrap(inputBytes, 0, inputBytes.Length);

        Assert.AreEqual(HexConvertor.GetString(result), HexConvertor.GetString(outputBytes));
    }

    [DataTestMethod]
    [DataRow("aa0c6b94bcc98519a50a064622faf33e", "ef792023")]
    [DataRow("aa0c6b94bcc98519a50a064622faf33e", "295571f47bbc7f08f46a04a01e598794e2")]
    [DataRow("aa0c6b94bcc98519a50a064622faf33e", "f635f3749a18307d3e2c7909e6105d5884510b66f639125f4dc6214112ebb1ae1de2d4")]
    [DataRow("aa0c6b94bcc98519a50a064622faf33e", "73d4292807f5f53469791261b0353741a013811e1e11a0c2949812cc55c6ebb8fc1c5bf3eadfaae9c7326d6c1fa3b38e0c0f78b4333e57bc583868a7eb9a4f2c82f7e3405b3172d7f66818a947b527b2e0b91bcb6fd44f8d782211c91741d49517440b3ec8")]
    public void WrapAndUnwrapData_Aes_Success(string key, string input)
    {
        byte[] keyBytes = HexConvertor.GetBytes(key);
        byte[] inputBytes = HexConvertor.GetBytes(input);

        Rfc5649WrapEngine engine = new Rfc5649WrapEngine(AesUtilities.CreateEngine());
        engine.Init(true, new KeyParameter(keyBytes));
        byte[] wrapedData = engine.Wrap(inputBytes, 0, inputBytes.Length);

        Rfc5649WrapEngine unwrapEngine = new Rfc5649WrapEngine(AesUtilities.CreateEngine());
        unwrapEngine.Init(false, new KeyParameter(keyBytes));
        byte[] result = unwrapEngine.Unwrap(wrapedData, 0, wrapedData.Length);

        Assert.AreEqual(HexConvertor.GetString(result), HexConvertor.GetString(inputBytes));
    }
}
