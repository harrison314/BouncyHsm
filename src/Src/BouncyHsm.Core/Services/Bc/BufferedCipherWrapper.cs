using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Bc;

internal class BufferedCipherWrapper : IWrapper
{
    private readonly IBufferedCipher bufferedCipher;
    private ICipherParameters? parameters;
    private bool forWrapping;

    public string AlgorithmName
    {
        get => this.bufferedCipher.AlgorithmName;
    }

    public BufferedCipherWrapper(IBufferedCipher bufferedCipher, bool forWrapping = false)
    {
        this.bufferedCipher = bufferedCipher;
        this.forWrapping = forWrapping;
        this.parameters = null;
    }

    public void Init(bool forWrapping, ICipherParameters parameters)
    {
        System.Diagnostics.Debug.Assert(parameters != null);

        this.forWrapping = forWrapping;
        this.parameters = parameters;
    }

    public byte[] Wrap(byte[] input, int inOff, int length)
    {
        if (!this.forWrapping)
        {
            throw new InvalidOperationException("Wrap operation not initialized.");
        }

        System.Diagnostics.Debug.Assert(this.parameters != null);
        this.bufferedCipher.Init(true, this.parameters);
        return this.bufferedCipher.DoFinal(input, inOff, length);
    }

    public byte[] Unwrap(byte[] input, int inOff, int length)
    {
        if (this.forWrapping)
        {
            throw new InvalidOperationException("Unwrap operation not initialized.");
        }

        System.Diagnostics.Debug.Assert(this.parameters != null);
        this.bufferedCipher.Init(false, this.parameters);
        return this.bufferedCipher.DoFinal(input, inOff, length);
    }

    public override string ToString()
    {
        return $"Buffered cipher wrapper with {this.AlgorithmName}";
    }
}
