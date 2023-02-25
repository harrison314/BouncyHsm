using BouncyHsm.Core.Services.Bc;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.UseCases.Implementation.Generators;

internal class X509CertObjectGenerator
{
    private readonly X509CertificateWrapper x509CertificateWrapper;
    private readonly byte[] ckaId;
    private readonly string ckaLabel;

    public X509CertObjectGenerator(X509CertificateWrapper x509CertificateWrapper,
        byte[] ckaId,
        string ckaLabel)
    {
        this.x509CertificateWrapper = x509CertificateWrapper;
        this.ckaId = ckaId;
        this.ckaLabel = ckaLabel;
    }

    public X509CertObjectGenerator(byte[] content,
        byte[] ckaId,
        string ckaLabel)
    {
        this.x509CertificateWrapper = X509CertificateWrapper.FromInstance(content);
        this.ckaId = ckaId;
        this.ckaLabel = ckaLabel;
    }

    public X509CertificateObject CreateCertificateObject(bool isCaCert)
    {
        X509CertificateObject certificateObject = new X509CertificateObject();
        certificateObject.CkaCertificateCategory = isCaCert
            ? CKCertificateCategory.CK_CERTIFICATE_CATEGORY_AUTHORITY
            : CKCertificateCategory.CK_CERTIFICATE_CATEGORY_TOKEN_USER;

        certificateObject.CkaCopyable = true;
        certificateObject.CkaDestroyable = true;
        certificateObject.CkaEndDate = this.x509CertificateWrapper.CkaEndDate;
        certificateObject.CkaId = this.ckaId;
        certificateObject.CkaIssuer = this.x509CertificateWrapper.CkaIssuer;
        certificateObject.CkaLabel = this.ckaLabel;
        certificateObject.CkaModifiable = false;
        certificateObject.CkaNameHashAlgorithm = CKM.CKM_SHA_1;
        certificateObject.CkaPrivate = false;
        certificateObject.CkaSerialNumber = this.x509CertificateWrapper.CkaSerialNumber;
        certificateObject.CkaStartDate = this.x509CertificateWrapper.CkaStartDate;
        certificateObject.CkaSubject = this.x509CertificateWrapper.CkaSubject;
        certificateObject.CkaToken = true;
        certificateObject.CkaTrusted = false;
        certificateObject.CkaValue = this.x509CertificateWrapper.CkaValue;

        certificateObject.ReComputeAttributes();

        return certificateObject;
    }
}
