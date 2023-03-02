using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.UseCases.Contracts;

public class CertificateDetail
{
    public string Subject
    {
        get;
        set;
    }

    public string Issuer
    {
        get;
        set;
    }

    public DateTime NotAfter
    {
        get;
        set;
    }

    public DateTime NotBefore
    {
        get;
        set;
    }

    public string SerialNumber
    {
        get;
        set;
    }

    public string Thumbprint
    {
        get;
        set;
    }

    public string SignatureAlgorithm
    {
        get;
        set;
    }

    public CertificateDetail()
    {
        this.Subject = string.Empty;
        this.Issuer = string.Empty;
        this.SerialNumber = string.Empty;
        this.Thumbprint = string.Empty;
        this.SignatureAlgorithm = string.Empty;
    }
}
