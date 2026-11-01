using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace BouncyHsm.Core.Services.Bc;

public sealed class Pkcs11CtrBlockCipher : IBlockCipherMode
{
    private readonly IBlockCipher cipher;

    private byte[] counter;
    private byte[] iv;
    private byte[] counterOut;

    private int counterBits;
    private bool initialised;

    public string AlgorithmName
    {
        get
        {
            return $"{this.cipher.AlgorithmName}/PKCS11CTR";
        }
    }

    public bool IsPartialBlockOkay
    {
        get => true;
    }

    public IBlockCipher UnderlyingCipher
    {
        get => this.cipher;
    }

    public Pkcs11CtrBlockCipher(IBlockCipher cipher)
    {
        this.cipher = cipher;

        if (cipher.GetBlockSize() != 16)
        {
            throw new ArgumentException("Only 128-bit block ciphers are supported");
        }

        this.counter = Array.Empty<byte>();
        this.iv = Array.Empty<byte>();
        this.counterOut = Array.Empty<byte>();
    }


    public int GetBlockSize()
    {
        return 16;
    }

    public void Init(bool forEncryption, ICipherParameters parameters)
    {
        if (parameters is not ParametersWithIV p)
        {
            throw new ArgumentException("ParametersWithIV required");
        }

        if (p.IVLength != 16)
        {
            throw new ArgumentException("CTR block must be 16 bytes");
        }

        this.counterBits = 128;

        if (parameters is ParametersWithIVAndCounterBits pc)
        {
            this.counterBits = pc.CounterBits;
            p = pc.ParametersWithIV;
        }

        this.counter = p.GetIV();
        this.counterOut = new byte[16];

        this.cipher.Init(true, p.Parameters);

        this.initialised = true;
    }

    public int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
    {
        this.CheckInitialised();

        this.cipher.ProcessBlock(this.counter, 0, this.counterOut, 0);

        for (int i = 0; i < 16; i++)
        {
            output[outOff + i] = (byte)(input[inOff + i] ^ this.counterOut[i]);
        }

        this.IncrementCounter();

        return 16;
    }

    public int ProcessBlock(ReadOnlySpan<byte> input, Span<byte> output)
    {
        this.CheckInitialised();

        this.cipher.ProcessBlock(this.counter, 0, this.counterOut, 0);

        for (int i = 0; i < 16; i++)
        {
            output[i] = (byte)(input[i] ^ this.counterOut[i]);
        }

        this.IncrementCounter();

        return 16;
    }

    public void Reset()
    {
        Arrays.Fill(this.counter, (byte)0);
        Array.Copy(this.iv, 0, this.counter, 0, this.iv.Length);
    }

    private void CheckInitialised()
    {
        if (!this.initialised)
        {
            throw new InvalidOperationException();
        }
    }

    private void IncrementCounter()
    {
        int counterBytes = (this.counterBits + 7) / 8;

        int bytePos = 15;
        int remainingBits = this.counterBits;

        while (remainingBits >= 8)
        {
            if (++this.counter[bytePos] != 0)
            {
                return;
            }

            remainingBits -= 8;
            bytePos--;
        }

        if (remainingBits > 0)
        {
            int mask = (1 << remainingBits) - 1;
            int value = this.counter[bytePos] & mask;
            value = (value + 1) & mask;
            this.counter[bytePos] = (byte)((this.counter[bytePos] & ~mask) | value);
        }
    }
}