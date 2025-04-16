using BouncyHsm.Core.Services.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.SpeedAwaiters;

internal class GenerateKeyVisitor : BaseKeyTimeVisitor
{
    private readonly double[] polynomialMultiplication;

    public GenerateKeyVisitor(double[] polynomialMultiplication)
    {
        this.polynomialMultiplication = polynomialMultiplication;
    }

    public override TimeSpan Visit(RsaPrivateKeyObject rsaPrivateKeyObject)
    {
        int keySize = rsaPrivateKeyObject.CkaModulus.Length * 8;
        double x = keySize;

        double result = 0.0;

        result += ((21.0 * x * x * x) / 268435456.0) * this.GetMultiplicator(3);
        result += ((x * x) / 131072.0) * this.GetMultiplicator(2);
        result += ((52.0 * x) / 128.0) * this.GetMultiplicator(1);
        result += 25.7 * this.GetMultiplicator(0);

        return TimeSpan.FromMilliseconds(result);
    }

    public override TimeSpan Visit(EcdsaPrivateKeyObject ecdsaPrivateKeyObject)
    {
        int keySize = ecdsaPrivateKeyObject.CkaValue.Length * 8;

        double x = keySize;

        double result = 0.0;
        result += ((115.0 * x * x) / 16384.0) * this.GetMultiplicator(2);
        result += ((-577.0 * x) / 256.0) * this.GetMultiplicator(1);
        result += 24.0 * this.GetMultiplicator(0);

        return TimeSpan.FromMilliseconds(result);
    }

    public override TimeSpan Visit(GenericSecretKeyObject generalSecretKeyObject)
    {
        return this.GetSimetricKeyTimeSpan(generalSecretKeyObject.CkaValueLen);
    }

    public override TimeSpan Visit(AesKeyObject aesKeyObject)
    {
        return this.GetSimetricKeyTimeSpan(aesKeyObject.CkaValueLen);
    }

    public override TimeSpan Visit(Poly1305KeyObject poly1305KeyObject)
    {
        return this.GetSimetricKeyTimeSpan(poly1305KeyObject.CkaValueLen);
    }

    private double GetMultiplicator(int pi)
    {
        if (pi < this.polynomialMultiplication.Length)
        {
            return this.polynomialMultiplication[pi];
        }

        return 1.0;
    }

    private TimeSpan GetSimetricKeyTimeSpan(uint keySizeInBytes)
    {
        double result = keySizeInBytes * 8 * this.GetMultiplicator(0);
        return TimeSpan.FromMilliseconds(result);
    }
}
