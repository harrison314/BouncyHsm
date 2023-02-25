using BouncyHsm.Core.Services.Contracts;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.States;

internal class DigestSessionState : ISessionState
{
    private readonly IDigest digest;

    public uint DigestLength
    {
        get => Convert.ToUInt32(this.digest.GetDigestSize());
    }

    public bool IsUpdated
    {
        get;
        private set;
    }

    public DigestSessionState(IDigest digest)
    {
        System.Diagnostics.Debug.Assert(digest != null);

        this.digest = digest;
    }

    public void Update(byte[] data)
    {
        System.Diagnostics.Debug.Assert(data != null);

        this.digest.BlockUpdate(data);

        if (data.Length > 0)
        {
            this.IsUpdated = true;
        }
    }

    public byte[] Final()
    {
        if (!this.IsUpdated)
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_GENERAL_ERROR, "Error: Digesting empty data.");
        }

        byte[] buffer = new byte[this.digest.GetDigestSize()];
        this.digest.DoFinal(buffer);

        return buffer;
    }

    public override string ToString()
    {
        return $"DigestState: IsUpdated {this.IsUpdated}, Digest: {this.digest.AlgorithmName}.";
    }
}
