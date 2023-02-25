using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Prng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace BouncyHsm.Core.Services.Bc;

public class HwRandomGenerator : IRandomGenerator
{
    public static SecureRandom SecureRandom
    {
        get;
    } = new SecureRandom(new HwRandomGenerator());

    public HwRandomGenerator()
    {

    }

    public void AddSeedMaterial(byte[] seed)
    {
        //NOP
    }

    public void AddSeedMaterial(ReadOnlySpan<byte> seed)
    {
        //NOP
    }

    public void AddSeedMaterial(long seed)
    {
        //NOP
    }

    public void NextBytes(byte[] bytes)
    {
        RandomNumberGenerator.Fill(bytes.AsSpan());
    }

    public void NextBytes(byte[] bytes, int start, int len)
    {
        RandomNumberGenerator.Fill(bytes.AsSpan(start, len));
    }

    public void NextBytes(Span<byte> bytes)
    {
        RandomNumberGenerator.Fill(bytes);
    }
}
