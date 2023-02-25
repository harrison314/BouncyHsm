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
    private bool isEmpty;

    public VerifyState(ISigner signer)
    {
        System.Diagnostics.Debug.Assert(signer != null);

        this.signer = signer;
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

        if (this.isEmpty)
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_GENERAL_ERROR, "Error: Verify empty data.");
        }

       return this.signer.VerifySignature(signature);
    }

    public override string ToString()
    {
        return $"Verify state - algorithm: {this.signer.AlgorithmName}";
    }
}
