using Microsoft.VisualStudio.TestTools.UnitTesting;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Security.Cryptography.ECCurve;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T23_DeriveKeyMongomery
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [DataTestMethod]
    [DataRow("id-X25519", "C7E0D5DEB74F5553CA335A7BE15187BC3C09CE8BA6C467856663CE1D47A9C576")]
    [DataRow("id-X448", "8F5321C752DFF872CD7A32D9F4F0878AACB4F228B3FD94C35AE64935B9604AA0EE23E168888CC5249E0F78371BA507CBBEE8D9788B68EDC3")]
    public void Derive_Edwards_ECDH1NULL_Success(string curveName, string publicKeyHex)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        (IObjectHandle publicKey, IObjectHandle privateKey) = this.GeneareMongomeryKeyPair(session, curveName);

        List<IObjectAttribute> deriveAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, 32)
        };

        byte[] otherEcPoint = Convert.FromHexString(publicKeyHex);
        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkEcdh1DeriveParams mp = factories.MechanismParamsFactory.CreateCkEcdh1DeriveParams((ulong)CKD.CKD_NULL,
            null,
            otherEcPoint);
        using IMechanism deriveMechanism = factories.MechanismFactory.Create(CKM.CKM_ECDH1_DERIVE, mp);

        IObjectHandle derivedHandle = session.DeriveKey(deriveMechanism, privateKey, deriveAttributes);

        byte[] derivedValue = this.GetValue(session, derivedHandle);
    }

    [DataTestMethod]
    [DataRow("id-X25519", CKD.CKD_SHA1_KDF, "C7E0D5DEB74F5553CA335A7BE15187BC3C09CE8BA6C467856663CE1D47A9C576", "")]
    [DataRow("id-X25519", CKD.CKD_SHA1_KDF, "C7E0D5DEB74F5553CA335A7BE15187BC3C09CE8BA6C467856663CE1D47A9C576", "-")]
    [DataRow("id-X25519", CKD.CKD_SHA1_KDF, "C7E0D5DEB74F5553CA335A7BE15187BC3C09CE8BA6C467856663CE1D47A9C576", "C7E0D5DEB74F")]
    [DataRow("id-X25519", CKD.CKD_SHA256_KDF, "C7E0D5DEB74F5553CA335A7BE15187BC3C09CE8BA6C467856663CE1D47A9C576", "")]
    [DataRow("id-X25519", CKD.CKD_SHA256_KDF, "C7E0D5DEB74F5553CA335A7BE15187BC3C09CE8BA6C467856663CE1D47A9C576", "-")]
    [DataRow("id-X25519", CKD.CKD_SHA256_KDF, "C7E0D5DEB74F5553CA335A7BE15187BC3C09CE8BA6C467856663CE1D47A9C576", "C7E0D5DEB74F5553CA335A7BE15187BC3C09CE8BA6C467856663CE1D47A9C576")]
    [DataRow("id-X25519", CKD_V3_0.CKD_SHA3_512_KDF, "C7E0D5DEB74F5553CA335A7BE15187BC3C09CE8BA6C467856663CE1D47A9C576", "C7E0D5DEB74F")]
    [DataRow("id-X25519", CKD.CKD_SHA512_KDF, "C7E0D5DEB74F5553CA335A7BE15187BC3C09CE8BA6C467856663CE1D47A9C576", "")]
    [DataRow("id-X448", CKD.CKD_SHA1_KDF, "8F5321C752DFF872CD7A32D9F4F0878AACB4F228B3FD94C35AE64935B9604AA0EE23E168888CC5249E0F78371BA507CBBEE8D9788B68EDC3", "")]
    [DataRow("id-X448", CKD.CKD_SHA1_KDF, "8F5321C752DFF872CD7A32D9F4F0878AACB4F228B3FD94C35AE64935B9604AA0EE23E168888CC5249E0F78371BA507CBBEE8D9788B68EDC3", "-")]
    [DataRow("id-X448", CKD.CKD_SHA1_KDF, "8F5321C752DFF872CD7A32D9F4F0878AACB4F228B3FD94C35AE64935B9604AA0EE23E168888CC5249E0F78371BA507CBBEE8D9788B68EDC3", "C7E0D5DEB74F5553CA335A7BE15187BC3C09CE8BA6C467856663CE1D47A9C576")]
    [DataRow("id-X448", CKD.CKD_SHA256_KDF, "8F5321C752DFF872CD7A32D9F4F0878AACB4F228B3FD94C35AE64935B9604AA0EE23E168888CC5249E0F78371BA507CBBEE8D9788B68EDC3", "C7E0D5DEB74F5553CA335A7BE15187BC3C09CE8BA6C467856663CE1D47A9C576")]
    [DataRow("id-X448", CKD.CKD_SHA512_KDF, "8F5321C752DFF872CD7A32D9F4F0878AACB4F228B3FD94C35AE64935B9604AA0EE23E168888CC5249E0F78371BA507CBBEE8D9788B68EDC3", "C7E0D5DEB74F5553CA335A7BE15187BC3C09CE8BA6C467856663CE1D47A9C576")]
    [DataRow("id-X448", CKD.CKD_SHA512_KDF, "8F5321C752DFF872CD7A32D9F4F0878AACB4F228B3FD94C35AE64935B9604AA0EE23E168888CC5249E0F78371BA507CBBEE8D9788B68EDC3", "C7E0D5DEB74F5553CA335A7BE15187BC3C09CE8BA6C467856663CE1D47A9C576")]
    public void Derive_Edwards_ECDH1_Success(string curveName, CKD derivationFn, string publicKeyHex, string additionalData)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        (IObjectHandle publicKey, IObjectHandle privateKey) = this.GeneareMongomeryKeyPair(session, curveName);

        List<IObjectAttribute> deriveAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, 32)
        };

        byte[] otherEcPoint = Convert.FromHexString(publicKeyHex);
        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkEcdh1DeriveParams mp = factories.MechanismParamsFactory.CreateCkEcdh1DeriveParams((ulong)derivationFn,
            this.GetAaadData(additionalData),
            otherEcPoint);
        using IMechanism deriveMechanism = factories.MechanismFactory.Create(CKM.CKM_ECDH1_DERIVE, mp);

        IObjectHandle derivedHandle = session.DeriveKey(deriveMechanism, privateKey, deriveAttributes);

        byte[] derivedValue = this.GetValue(session, derivedHandle);
    }

    private byte[]? GetAaadData(string aadDataHex)
    {
        if (string.IsNullOrEmpty(aadDataHex))
        {
            return Array.Empty<byte>();
        }

        if (aadDataHex == "-")
        {
            return null;
        }

        return Convert.FromHexString(aadDataHex);
    }

    private byte[] GetValue(ISession session, IObjectHandle handle, CKA attr = CKA.CKA_VALUE)
    {
        return session.GetAttributeValue(handle, new List<CKA>() { attr })[0].GetValueAsByteArray();
    }

    private (IObjectHandle publicKey, IObjectHandle privateKey) GeneareMongomeryKeyPair(ISession session,
        string curveName)
    {
        string label = $"X-KeyTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);
        byte[] namedCurve = new DerPrintableString(curveName).GetEncoded();

        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY_RECOVER, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, namedCurve),
        };

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true)
        };

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM_V3_0.CKM_EC_MONTGOMERY_KEY_PAIR_GEN);

        session.GenerateKeyPair(mechanism,
            publicKeyAttributes,
            privateKeyAttributes,
            out IObjectHandle publicKey,
            out IObjectHandle privateKey);

        return (publicKey, privateKey);
    }
}
