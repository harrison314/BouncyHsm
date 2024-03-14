using BouncyHsm.Core.Services.Bc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Core.UseCases.Implementation.Generators;
using BouncyHsm.Core.UseCases.Implementation.Visitors;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using System.Collections.Generic;
using System.Linq;

namespace BouncyHsm.Core.UseCases.Implementation;

public class PkcsFacade : IPkcsFacade
{
    private readonly IPersistentRepository persistentRepository;
    private readonly ITimeAccessor timeAccessor;
    private readonly ILogger<PkcsFacade> logger;

    public PkcsFacade(IPersistentRepository persistentRepository, ITimeAccessor timeAccessor, ILogger<PkcsFacade> logger)
    {
        this.persistentRepository = persistentRepository;
        this.timeAccessor = timeAccessor;
        this.logger = logger;
    }

    public async ValueTask<DomainResult<Guid>> ImportP12(ImportP12Request request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to ImportP12");

        Services.Contracts.Entities.SlotEntity? slot = await this.persistentRepository.GetSlot(request.SlotId, cancellationToken);
        if (slot == null)
        {
            this.logger.LogError("SlotId {slotId} not found.", request.SlotId);
            return new DomainResult<Guid>.InvalidInput("SlotId not found.");
        }

        if (request.ImportMode == P12ImportMode.LocalInQualifiedArea && !slot.Token.SimulateQualifiedArea)
        {
            this.logger.LogError("SlotId {slotId} not contains token with qualified area.", request.SlotId);
            return new DomainResult<Guid>.InvalidInput("SlotId  not contains token with qualified area.");
        }

        try
        {
            P12ObjectsGenerator p12ObjectsGenerator = new P12ObjectsGenerator(request.Pkcs12Content,
                request.Password,
                request.CkaLabel,
                request.CkaId);
            p12ObjectsGenerator.ImportMode = request.ImportMode;

            List<StorageObject> objects = new List<StorageObject>
        {
            p12ObjectsGenerator.CreatePrivateKey(),
            p12ObjectsGenerator.CreatePublicKey(),
            p12ObjectsGenerator.CreateCertificate()
        };

            if (request.ImportChain)
            {
                objects.AddRange(p12ObjectsGenerator.GetCertificateChain());
            }

            foreach (StorageObject storageObject in objects)
            {
                storageObject.Validate();
            }


            foreach (StorageObject storageObject in objects)
            {
                await this.persistentRepository.StoreObject(request.SlotId,
                    storageObject,
                    cancellationToken);
            }

            this.logger.LogInformation("Successfull imported Pkcs12 file into private key {privateKeyId}, publicKey {publicKeyId} and certificate {certificateId}.",
                objects[0].Id,
                objects[1].Id,
                objects[2].Id);

            return new DomainResult<Guid>.Ok(objects[0].Id);
        }
        catch (System.IO.IOException ex) when (ex.Message.Contains("MAC invalid"))
        {
            this.logger.LogError(ex, "Probably invalid password for P12.");
            return new DomainResult<Guid>.InvalidInput("Invalid password.");
        }
        catch (BouncyHsmInvalidInputException ex)
        {
            this.logger.LogError(ex, "Invalid input.");
            return new DomainResult<Guid>.InvalidInput(ex.Message);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unexecuted exception during import p12 file.");
            throw;
        }
    }

    public async ValueTask<DomainResult<PkcsObjects>> GetObjects(uint slotId, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GetObjects with slotId {slotId}.", slotId);

        IEnumerable<PrivateKeyObject> privateKeys = await this.FindObjects<PrivateKeyObject>(slotId, CKO.CKO_PRIVATE_KEY, cancellationToken);
        IEnumerable<PublicKeyObject> publicKeys = await this.FindObjects<PublicKeyObject>(slotId, CKO.CKO_PUBLIC_KEY, cancellationToken);
        IEnumerable<X509CertificateObject> certificates = await this.FindObjects<X509CertificateObject>(slotId,
            CKO.CKO_CERTIFICATE,
            cancellationToken,
            new KeyValuePair<CKA, IAttributeValue>(CKA.CKA_CERTIFICATE_TYPE, AttributeValue.Create((uint)CKC.CKC_X_509)));

        StorageObjectDescriptionVisitor descriptionVisitor = new StorageObjectDescriptionVisitor();

        List<PkcsObjectInfo> objects = privateKeys.Select(t => new
        {
            key = new LabelIdPair(t.CkaLabel, t.CkaId),
            type = CKO.CKO_PRIVATE_KEY,
            id = t.Id,
            alwaysAuthenticate = t.CkaAlwaysAuthenticate,
            description = t.Accept(descriptionVisitor),
            subject = null as string
        })
            .Concat(publicKeys.Select(t => new
            {
                key = new LabelIdPair(t.CkaLabel, t.CkaId),
                type = CKO.CKO_PUBLIC_KEY,
                id = t.Id,
                alwaysAuthenticate = false,
                description = t.Accept(descriptionVisitor),
                subject = null as string
            }))
            .Concat(certificates.Select(t => new
            {
                key = new LabelIdPair(t.CkaLabel, t.CkaId),
                type = CKO.CKO_CERTIFICATE,
                id = t.Id,
                alwaysAuthenticate = false,
                description = t.Accept(descriptionVisitor),
                subject = this.TryParseCertSubject(t)
            }))
            .GroupBy(t => t.key)
            .Select(t => new PkcsObjectInfo()
            {
                CkaLabel = t.Key.CkaLabel,
                CkaId = t.Key.CkaId,
                Objects = t.Select(q => new PkcsSpecificObject(q.type, q.id, q.description)).OrderByDescending(o => (uint)o.CkaClass).ToArray(),
                AlwaysAuthenticate = t.Any(q => q.alwaysAuthenticate),
                Subject = t.Select(q => q.subject).FirstOrDefault(q => q != null)
            })
            .ToList();

        this.logger.LogTrace("Found {count} PKCS objects.", objects.Count);

        return new DomainResult<PkcsObjects>.Ok(new PkcsObjects(objects));
    }

    public async ValueTask<DomainResult<byte[]>> GeneratePkcs10(GeneratePkcs10Request request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GetObjects with slotId {slotId}, PrivateKeyId {PrivateKeyId}, PublicKeyId {PublicKeyId}.",
            request.SlotId,
            request.PrivateKeyId,
            request.PublicKeyId);

        StorageObject? privSo = await this.persistentRepository.TryLoadObject(request.SlotId, request.PrivateKeyId, cancellationToken);
        if (privSo == null)
        {
            this.logger.LogError("Private key not found. SlotId {slotId}, object id {objectId}.", request.SlotId, request.PrivateKeyId);
            return new DomainResult<byte[]>.NotFound();
        }

        PrivateKeyObject? privKo = privSo as PrivateKeyObject;
        if (privKo == null)
        {
            this.logger.LogError("Object in slotId {slotId}, object id {objectId} is not private key.", request.SlotId, request.PrivateKeyId);
            return new DomainResult<byte[]>.InvalidInput("PrivateKeyId is not private key.");
        }

        StorageObject? pubSo = await this.persistentRepository.TryLoadObject(request.SlotId, request.PublicKeyId, cancellationToken);
        if (pubSo == null)
        {
            this.logger.LogError("Public key not found. SlotId {slotId}, object id {objectId}.", request.SlotId, request.PublicKeyId);
            return new DomainResult<byte[]>.NotFound();
        }

        PublicKeyObject? pubKo = pubSo as PublicKeyObject;
        if (pubKo == null)
        {
            this.logger.LogError("Object in slotId {slotId}, object id {objectId} is not public key.", request.SlotId, request.PublicKeyId);
            return new DomainResult<byte[]>.InvalidInput("PublicKeyId is not public key.");
        }

        X509Name subject = request.Subject.Match(text => new X509Name(dirName: text.X509NameText),
            oidValuePairs => new X509Name(oidValuePairs.Pairs.Select(t => new Org.BouncyCastle.Asn1.DerObjectIdentifier(t.Oid)).ToList(),
                 oidValuePairs.Pairs.Select(t => t.Value).ToList()));

        string algorithm = privKo.CkaKeyType switch
        {
            CKK.CKK_RSA => "SHA224WITHRSA",
            CKK.CKK_ECDSA => "SHA256WITHECDSA",
            _ => throw new InvalidProgramException($"Enum value {privKo.CkaKeyType} is not supported.")
        };

        Pkcs10CertificationRequest certificationRequest = new Pkcs10CertificationRequest(algorithm,
            subject,
            pubKo.GetPublicKey(),
            null,
            privKo.GetPrivateKey());

        return new DomainResult<byte[]>.Ok(certificationRequest.GetEncoded());
    }

    public async ValueTask<DomainResult<Guid>> GenerateSelfSignedCert(GenerateSelfSignedCertRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GenerateSelfSignedCert with slotId {slotId}, PrivateKeyId {PrivateKeyId}, PublicKeyId {PublicKeyId}.",
            request.SlotId,
            request.PrivateKeyId,
            request.PublicKeyId);

        if (request.Validity < TimeSpan.Zero)
        {
            this.logger.LogError("Validity is less than zero.");
            return new DomainResult<Guid>.InvalidInput("Validity is less than zero.");
        }

        StorageObject? privSo = await this.persistentRepository.TryLoadObject(request.SlotId, request.PrivateKeyId, cancellationToken);
        if (privSo == null)
        {
            this.logger.LogError("Private key not found. SlotId {slotId}, object id {objectId}.", request.SlotId, request.PrivateKeyId);
            return new DomainResult<Guid>.NotFound();
        }

        PrivateKeyObject? privKo = privSo as PrivateKeyObject;
        if (privKo == null)
        {
            this.logger.LogError("Object in slotId {slotId}, object id {objectId} is not private key.", request.SlotId, request.PrivateKeyId);
            return new DomainResult<Guid>.InvalidInput("PrivateKeyId is not private key.");
        }

        StorageObject? pubSo = await this.persistentRepository.TryLoadObject(request.SlotId, request.PublicKeyId, cancellationToken);
        if (pubSo == null)
        {
            this.logger.LogError("Public key not found. SlotId {slotId}, object id {objectId}.", request.SlotId, request.PublicKeyId);
            return new DomainResult<Guid>.NotFound();
        }

        PublicKeyObject? pubKo = pubSo as PublicKeyObject;
        if (pubKo == null)
        {
            this.logger.LogError("Object in slotId {slotId}, object id {objectId} is not public key.", request.SlotId, request.PublicKeyId);
            return new DomainResult<Guid>.InvalidInput("PublicKeyId is not public key.");
        }

        X509Name subject = request.Subject.Match(text => new X509Name(dirName: text.X509NameText),
            oidValuePairs => new X509Name(oidValuePairs.Pairs.Select(t => new Org.BouncyCastle.Asn1.DerObjectIdentifier(t.Oid)).ToList(),
                 oidValuePairs.Pairs.Select(t => t.Value).ToList()));

        string algorithm = privKo.CkaKeyType switch
        {
            CKK.CKK_RSA => "SHA224WITHRSA",
            CKK.CKK_ECDSA => "SHA256WITHECDSA",
            _ => throw new InvalidProgramException($"Enum value {privKo.CkaKeyType} is not supported.")
        };

        Asn1SignatureFactory asn1SignatureFactory = new Asn1SignatureFactory(algorithm,
            privKo.GetPrivateKey());

        DateTime utcNow = this.timeAccessor.UtcNow;
        X509V3CertificateGenerator generator = new X509V3CertificateGenerator();
        generator.SetIssuerDN(subject);
        generator.SetSubjectDN(subject);
        generator.SetSerialNumber(Org.BouncyCastle.Math.BigInteger.One);
        generator.SetPublicKey(pubKo.GetPublicKey());
        generator.SetNotBefore(utcNow);
        generator.SetNotAfter(utcNow.Add(request.Validity));
        generator.AddExtension(X509Extensions.KeyUsage, false, this.CreateKeyUsage(privKo));
        X509Certificate certificate = generator.Generate(asn1SignatureFactory);

        this.logger.LogDebug("Certificate generated.");

        X509CertificateWrapper certificateWrapper = X509CertificateWrapper.FromInstance(certificate);
        X509CertObjectGenerator objectGenerator = new X509CertObjectGenerator(certificateWrapper,
           privKo.CkaId,
           privKo.CkaLabel);

        X509CertificateObject certificateObject = objectGenerator.CreateCertificateObject(false);

        await this.persistentRepository.StoreObject(request.SlotId, certificateObject, cancellationToken);
        this.logger.LogInformation("Gneretae self-signed certificate and imported into slot {slotId} with object id {objectId}.",
            request.SlotId,
            certificateObject.Id);

        return new DomainResult<Guid>.Ok(certificateObject.Id);
    }

    public async ValueTask<DomainResult<Guid>> ImportX509Certificate(ImportX509CertificateRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to ImportX509Certificate with slotId {slotId}, PrivateKeyId {PrivateKeyId}.",
           request.SlotId,
           request.PrivateKeyId);

        StorageObject? privSo = await this.persistentRepository.TryLoadObject(request.SlotId, request.PrivateKeyId, cancellationToken);
        if (privSo == null)
        {
            this.logger.LogError("Private key not found. SlotId {slotId}, object id {objectId}.", request.SlotId, request.PrivateKeyId);
            return new DomainResult<Guid>.NotFound();
        }

        PrivateKeyObject? privKo = privSo as PrivateKeyObject;
        if (privKo == null)
        {
            this.logger.LogError("Object in slotId {slotId}, object id {objectId} is not private key.", request.SlotId, request.PrivateKeyId);
            return new DomainResult<Guid>.InvalidInput("PrivateKeyId is not private key.");
        }

        X509CertificateWrapper certificate;
        try
        {
            certificate = X509CertificateWrapper.FromInstance(request.Certificate);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Problem during parse X509 certificate.");
            return new DomainResult<Guid>.InvalidInput("Certificate is not valid X509 certificate.");
        }

        if (certificate.KeyType != privKo.CkaKeyType)
        {
            this.logger.LogError("Imported certificate is not match from private key. Certificate has key type {CertificateKeyType} private key type is {PrivateKeyType}.",
                certificate.KeyType,
                privKo.CkaKeyType);
            return new DomainResult<Guid>.InvalidInput("Imported certificate is not match from private key.");
        }

        if (!certificate.CheckPrivateKey(privKo.GetPrivateKey()))
        {
            this.logger.LogError("Imported certificate is not match from private key. Keys not match.");
            return new DomainResult<Guid>.InvalidInput("Imported certificate is not match from private key.");
        }

        X509CertObjectGenerator generator = new X509CertObjectGenerator(certificate,
            privKo.CkaId,
            privKo.CkaLabel);

        X509CertificateObject certificateObject = generator.CreateCertificateObject(false);

        await this.persistentRepository.StoreObject(request.SlotId, certificateObject, cancellationToken);
        this.logger.LogInformation("Certificate imported into slot {slotId} with object id {objectId}.",
            request.SlotId,
            certificateObject.Id);

        return new DomainResult<Guid>.Ok(certificateObject.Id);
    }

    public async ValueTask<VoidDomainResult> DeleteAssociatedObjects(uint slotId, Guid objectId, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to DeleteAssociatedObjects with objectId {objectId}.", objectId);

        StorageObject? storageObject = await this.persistentRepository.TryLoadObject(slotId, objectId, cancellationToken);
        if (storageObject == null)
        {
            this.logger.LogError("Object with id {objectId} not found.", objectId);
            return new VoidDomainResult.NotFound();
        }

        AttributeValueResult idResult = storageObject.GetValue(CKA.CKA_ID);
        if (!idResult.IsOK(out IAttributeValue? ckaId))
        {
            return new VoidDomainResult.InvalidInput("Object is not PKCS object (does not contains CKA_ID).");
        }

        Dictionary<CKA, IAttributeValue> searchTemplate = new Dictionary<CKA, IAttributeValue>()
        {
            {CKA.CKA_LABEL, AttributeValue.Create(storageObject.CkaLabel) },
            {CKA.CKA_ID, ckaId }
        };

        FindObjectSpecification specification = new FindObjectSpecification(searchTemplate, true);
        IReadOnlyList<StorageObject> foundObjects = await this.persistentRepository.FindObjects(slotId, specification, cancellationToken);

        foreach (StorageObject foundObject in foundObjects)
        {
            await this.persistentRepository.DestroyObject(slotId, foundObject, cancellationToken);
            this.logger.LogInformation("Delete object with id {objectId}.", foundObject.Id);
        }

        return new VoidDomainResult.Ok();
    }

    public async ValueTask<DomainResult<CertificateDetail>> ParseCertificate(uint slotId, Guid objectId, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to ParseCertificate with slotId {slotId}, objectId {objectId}.", slotId, objectId);

        StorageObject? storageObject = await this.persistentRepository.TryLoadObject(slotId, objectId, cancellationToken);
        if (storageObject == null)
        {
            this.logger.LogError("Certificate not found. SlotId {slotId}, object id {objectId}.", slotId, objectId);
            return new DomainResult<CertificateDetail>.NotFound();
        }

        X509CertificateObject? certificateObject = storageObject as X509CertificateObject;
        if (certificateObject == null)
        {
            this.logger.LogError("Object in slotId {slotId}, object id {objectId} is not X509Certificate.", slotId, objectId);
            return new DomainResult<CertificateDetail>.InvalidInput("PrivateKeyId is not X509Certificate.");
        }

        X509CertificateParser parser = new X509CertificateParser();
        X509Certificate certificate = parser.ReadCertificate(certificateObject.CkaValue);

        CertificateDetail result = new CertificateDetail()
        {
            Subject = certificate.SubjectDN.ToString(),
            Issuer = certificate.IssuerDN.ToString(),
            NotAfter = certificate.NotAfter.ToUniversalTime(),
            NotBefore = certificate.NotBefore.ToUniversalTime(),
            SerialNumber = certificate.SerialNumber.ToString(16),
            Thumbprint = certificate.GetThumbprint(),
            SignatureAlgorithm = certificate.SigAlgName
        };

        return new DomainResult<CertificateDetail>.Ok(result);
    }

    private KeyUsage CreateKeyUsage(PrivateKeyObject keyObject)
    {
        int value = 0;
        if (keyObject.CkaDecrypt)
        {
            value |= KeyUsage.DataEncipherment;
        }

        if (keyObject.CkaUnwrap)
        {
            value |= KeyUsage.KeyEncipherment;
        }

        if (keyObject.CkaDerive)
        {
            value |= KeyUsage.KeyAgreement;
        }

        if (keyObject.CkaSign)
        {
            value |= KeyUsage.DigitalSignature | KeyUsage.NonRepudiation | KeyUsage.KeyCertSign | KeyUsage.CrlSign;
        }

        return new KeyUsage(value);
    }

    private string? TryParseCertSubject(X509CertificateObject? x509CertificateObject)
    {
        if (x509CertificateObject == null || x509CertificateObject.CkaValue.Length == 0)
        {
            return null;
        }

        X509CertificateParser parser = new X509CertificateParser();
        X509Certificate cert = parser.ReadCertificate(x509CertificateObject.CkaValue);

        return cert.SubjectDN.ToString();
    }

    private async ValueTask<IEnumerable<T>> FindObjects<T>(uint slotId, CKO cko, CancellationToken cancellationToken, params KeyValuePair<CKA, IAttributeValue>[] additionalConstraints)
        where T : StorageObject
    {
        this.logger.LogTrace("Entering to FindObjects with slotId {slotId}m cko {cko}.", slotId, cko);

        Dictionary<CKA, IAttributeValue> searchTemplate = new Dictionary<CKA, IAttributeValue>()
        {
            {CKA.CKA_CLASS, AttributeValue.Create((uint) cko) }
        };

        foreach ((CKA attrType, IAttributeValue attrValue) in additionalConstraints)
        {
            searchTemplate.Add(attrType, attrValue);
        }

        FindObjectSpecification specification = new FindObjectSpecification(searchTemplate, true);
        IReadOnlyList<StorageObject> result = await this.persistentRepository.FindObjects(slotId, specification, cancellationToken);
        return result.OfType<T>();
    }
}