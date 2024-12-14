using BouncyHsm.Core.Services.Contracts;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class Pkcs1DigestInfoCheckerAsDigest : IDigest
{
    private readonly MemoryStream buffer;
    private readonly ILogger<Pkcs1DigestInfoCheckerAsDigest> logger;

    public string AlgorithmName
    {
        get => "NULL";
    }

    public Pkcs1DigestInfoCheckerAsDigest(ILogger<Pkcs1DigestInfoCheckerAsDigest> logger)
    {
        this.buffer = new MemoryStream();
        this.logger = logger;
    }

    public int GetByteLength()
    {
        return 0;
    }

    public int GetDigestSize()
    {
        return Convert.ToInt32(this.buffer.Length);
    }

    public void Update(byte b)
    {
        this.buffer.WriteByte(b);
    }

    public void BlockUpdate(byte[] inBytes, int inOff, int len)
    {
        this.buffer.Write(inBytes, inOff, len);
    }

    public void BlockUpdate(ReadOnlySpan<byte> input)
    {
        this.buffer.Write(input);
    }

    public int DoFinal(byte[] outBytes, int outOff)
    {
        try
        {
            byte[] buffer = this.buffer.GetBuffer();
            int num = Convert.ToInt32(this.buffer.Length);
            this.CheckData(buffer.AsSpan(0, num));
            Array.Copy(buffer, 0, outBytes, outOff, num);
            return num;
        }
        finally
        {
            this.Reset();
        }
    }

    public int DoFinal(Span<byte> output)
    {
        try
        {
            byte[] buffer = this.buffer.GetBuffer();
            int num = Convert.ToInt32(this.buffer.Length);
            this.CheckData(buffer.AsSpan(0, num));
            buffer.AsSpan(0, num).CopyTo(output);
            return num;
        }
        finally
        {
            this.Reset();
        }
    }

    public void Reset()
    {
        this.buffer.SetLength(0L);
    }

    private void CheckData(Span<byte> data)
    {
        try
        {
            DigestInfo digestInfo = DigestInfo.GetInstance(data.ToArray());
            IDigest digestAlgorithm = DigestUtilities.GetDigest(digestInfo.DigestAlgorithm.Algorithm);
            if (digestAlgorithm == null)
            {
                this.logger.LogWarning("Unknown digest algoritm with oid {algoritm} for signing in PKCS1 DigestInfo. DigestInfo: {data}",
                    digestInfo.DigestAlgorithm.Algorithm,
                    Convert.ToBase64String(data, Base64FormattingOptions.InsertLineBreaks));
            }
            else
            {
                byte[] digest = digestInfo.GetDigest();
                int exceptedDigestSize = digestAlgorithm.GetDigestSize();

                if (digest.Length != exceptedDigestSize)
                {
                    this.logger.LogError("Invalid digest size ({digestSize}, excepted {exceptedDigestSize} in algorithm {algorithm}) for signing in PKCS1 DigestInfo. DigestInfo: {data}",
                        digest.Length,
                        exceptedDigestSize,
                        digestInfo.DigestAlgorithm.Algorithm,
                        Convert.ToBase64String(data, Base64FormattingOptions.InsertLineBreaks));

                    throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_DATA_INVALID, $"Invalid digest size in PKCS1 DigestInfo.");
                }

            }
        }
        catch (RpcPkcs11Exception)
        {
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex,
                "Invalid data structure of DigestInfo for signing. DigestInfo: {data}",
                Convert.ToBase64String(data, Base64FormattingOptions.InsertLineBreaks));

            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_DATA_INVALID, $"Invalid data structure of DigestInfo for signing.", ex);
        }
    }
}
