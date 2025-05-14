using BouncyHsm.Core.Services.Contracts;
using Org.BouncyCastle.Crypto;

namespace BouncyHsm.Core.Services.Bc;

internal class SafeRawAgreement : IRawAgreement
{
    private readonly IRawAgreement parent;

    public int AgreementSize
    {
        get => this.parent.AgreementSize;
    }

    public SafeRawAgreement(IRawAgreement parent)
    {
        this.parent = parent;
    }

    public void CalculateAgreement(ICipherParameters publicKey, byte[] buf, int off)
    {
        int buffLen = buf.Length - off;
        if (buffLen == this.parent.AgreementSize)
        {
            this.parent.CalculateAgreement(publicKey, buf, off);
        }
        else if (buffLen < this.parent.AgreementSize)
        {
            byte[] tmp = new byte[this.parent.AgreementSize];

            this.parent.CalculateAgreement(publicKey, tmp, 0);
            tmp.AsSpan(0, buffLen).CopyTo(buf.AsSpan(off));
        }
        else
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_TEMPLATE_INCONSISTENT,
                "Invalid CKA_VALUE_LEN for template in derivation.");
        }
    }

    public void CalculateAgreement(ICipherParameters publicKey, Span<byte> output)
    {
        if (output.Length == this.parent.AgreementSize)
        {
            this.parent.CalculateAgreement(publicKey, output);
        }
        else if (output.Length < this.parent.AgreementSize)
        {
            Span<byte> tmp = (this.parent.AgreementSize < 120)
                ? stackalloc byte[this.parent.AgreementSize]
                : new byte[this.parent.AgreementSize];

            this.parent.CalculateAgreement(publicKey, tmp);
            tmp.Slice(0, output.Length).CopyTo(output);
        }
        else
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_TEMPLATE_INCONSISTENT,
                "Invalid CKA_VALUE_LEN for template in derivation.");
        }
    }

    public void Init(ICipherParameters parameters)
    {
        this.parent.Init(parameters);
    }

    public override string ToString()
    {
        return $"Safe: {this.parent.ToString()}";
    }
}