using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Core.Services.P11Handlers.SpeedAwaiters;

internal class SignVisitor : BaseKeyTimeVisitor
{
    private readonly double[] polynomialMultiplication;

    public SignVisitor(double[] polynomialMultiplication)
    {
        this.polynomialMultiplication = polynomialMultiplication;
    }

    public override TimeSpan Visit(RsaPrivateKeyObject rsaPrivateKeyObject)
    {
        int keySize = rsaPrivateKeyObject.CkaModulus.Length;
        double x = keySize;

        double result = (x * x) * 1.25 * this.GetMultiplicator(2);
        result += ((52.0 * x) / 128.0) * this.GetMultiplicator(1);
        result += 32.0 * this.GetMultiplicator(0);

        return TimeSpan.FromMilliseconds(result);
    }

    public override TimeSpan Visit(EcdsaPrivateKeyObject ecdsaPrivateKeyObject)
    {
        int keySize = ecdsaPrivateKeyObject.CkaValue.Length;

        double x = keySize;

        double result = (x * x) * 5.0 * this.GetMultiplicator(2);
        result += (0.01 * x) * this.GetMultiplicator(1);
        result += 32.0 * this.GetMultiplicator(0);

        return TimeSpan.FromMilliseconds(result);
    }

    public override TimeSpan Visit(GenericSecretKeyObject generalSecretKeyObject)
    {
        double result = generalSecretKeyObject.CkaValueLen * 8 * this.GetMultiplicator(0);
        return TimeSpan.FromMilliseconds(result);
    }

    public override TimeSpan Visit(AesKeyObject aesKeyObject)
    {
        double result = aesKeyObject.CkaValueLen * 8 * this.GetMultiplicator(0);
        return TimeSpan.FromMilliseconds(result);
    }

    private double GetMultiplicator(int pi)
    {
        if (pi < this.polynomialMultiplication.Length)
        {
            return this.polynomialMultiplication[pi];
        }

        return 1.0;
    }
}