using BouncyHsm.Core.Services.Contracts;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.States;

internal class VerifyState : ISessionState
{
    private readonly ISigner signer;
    private readonly bool acceptEmptyData;
    private bool isEmpty;

    public VerifyState(ISigner signer, bool acceptEmptyData)
    {
        System.Diagnostics.Debug.Assert(signer != null);

        this.signer = signer;
        this.acceptEmptyData = acceptEmptyData;
        this.isEmpty = true;
    }

    public void Update(byte[] data)
    {
        System.Diagnostics.Debug.Assert(data != null);

        this.signer.BlockUpdate(data);

        if (data.Length > 0)
        {
            this.isEmpty = false;
        }
    }

    public bool Verify(byte[] signature)
    {
        System.Diagnostics.Debug.Assert(signature != null);

        if (this.isEmpty && !this.acceptEmptyData)
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_DATA_LEN_RANGE, "Error: Verify empty data.");
        }

        try
        {
            return this.signer.VerifySignature(signature);
        }
        catch(Org.BouncyCastle.Crypto.InvalidCipherTextException)
        {
            return false;
        }
        catch (Org.BouncyCastle.Crypto.DataLengthException)
        {
            return false;
        }
    }

    public override string ToString()
    {
        return $"Verify state - algorithm: {this.signer.AlgorithmName}";
    }
}
