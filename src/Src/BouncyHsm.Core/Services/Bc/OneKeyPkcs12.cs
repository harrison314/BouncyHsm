using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;

namespace BouncyHsm.Core.Services.Bc;

public class OneKeyPkcs12
{
    public AsymmetricKeyParameter PrivateKey
    {
        get;
    }

    public X509CertificateWrapper Certificate
    {
        get;
    }

    public X509CertificateWrapper[] CertificateChain
    {
        get;
    }

    public OneKeyPkcs12(byte[] content, string? password)
    {
        Pkcs12StoreBuilder builder = new Pkcs12StoreBuilder();
        Pkcs12Store store = builder.Build();

        using MemoryStream ms = new MemoryStream(content);
        try
        {
            store.Load(ms, password?.ToCharArray());
        }
        catch (ArgumentException ex)
        {
            throw new BouncyHsm.Core.Services.Contracts.BouncyHsmInvalidInputException("Content is not Pkcs12 store.", ex);
        }

        this.CheckStore(store);

        string alias = store.Aliases.Single();

        this.PrivateKey = store.GetKey(alias).Key;
        this.Certificate = X509CertificateWrapper.FromInstance(store.GetCertificate(alias).Certificate);
        this.CertificateChain = this.TranslateChain(store.GetCertificateChain(alias));
    }

    private void CheckStore(Pkcs12Store store)
    {
        int count;

        if (!store.Aliases.TryGetNonEnumeratedCount(out count))
        {
            count = store.Aliases.Count();
        }

        if (count != 1)
        {
            throw new ArgumentException("P12 must contains ony one key.", "content");
        }
    }

    private X509CertificateWrapper[] TranslateChain(X509CertificateEntry[]? x509CertificateEntries)
    {
        if (x509CertificateEntries == null || x509CertificateEntries.Length == 0)
        {
            return Array.Empty<X509CertificateWrapper>();
        }

        X509CertificateWrapper[] result = new X509CertificateWrapper[x509CertificateEntries.Length];
        for (int i = 0; i < x509CertificateEntries.Length; i++)
        {
            result[i] = X509CertificateWrapper.FromInstance(x509CertificateEntries[i].Certificate);
        }

        return result;
    }
}