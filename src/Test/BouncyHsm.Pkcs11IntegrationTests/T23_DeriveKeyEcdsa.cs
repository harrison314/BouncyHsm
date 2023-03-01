using Microsoft.VisualStudio.TestTools.UnitTesting;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Org.BouncyCastle.Asn1;
using PkcsExtensions;
using System.Security.Cryptography;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T23_DeriveKeyEcdsa
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    #region EC Values Constansts

    private const string P256_1_Ecparams = "06082A8648CE3D030107";
    private const string P256_1_EcPoint = "0441041296517BF7658A19CC81AEF155CA871B53598BCC92C5620D0540675C35DE0E91BECC04EDFE9411DD22ADF970CED18AF5480498E1B32E4C79F12BDC69FF086E50";
    private const string P256_1_EcValue = "A5A00402C8807452E299DC439C60921E9AD8F070F5A0BD48EC30C8CCCBBA8AB5";

    private const string P256_2_Ecparams = "06082A8648CE3D030107";
    private const string P256_2_EcPoint = "044104502B16314C1F60B03FC48F432A498E881D2B115FF851A400BE73831D6AF3C0B16573FEAEFCDE928B86CEF78AB71513505D330A06D3CE2C48FD158F9A4747A845";
    private const string P256_2_EcValue = "8716626FFE3001840C0D3D2FA29800EE5E1C72B023741B75041E1627B99752EF";

    private const string P256_1And2_DerivedNullValue = "3AA9B3F7A0CB6F82509A0C09655FD301F22C4A07B75032405BAB4E2CFC5BF961";
    #endregion

    [TestMethod]
    public void Derive_Ecdsa1_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        using ECDsa dotnetEcDsa = ECDsa.Create(System.Security.Cryptography.ECCurve.NamedCurves.nistP256);
        byte[] ecPointDotnet = this.ExtractEcPoint(dotnetEcDsa);

        byte[] p256EcParams = Org.BouncyCastle.Asn1.X9.ECNamedCurveTable.GetOid("P-256").GetEncoded();

        (IObjectHandle publicKey, IObjectHandle privateKey) = this.GenerateEcKeypair(session, p256EcParams);

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

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkEcdh1DeriveParams mp = factories.MechanismParamsFactory.CreateCkEcdh1DeriveParams((ulong)CKD.CKD_NULL,
            null,
            ecPointDotnet);
        using IMechanism deriveMechanism = factories.MechanismFactory.Create(CKM.CKM_ECDH1_DERIVE, mp);

        IObjectHandle derivedHandle = session.DeriveKey(deriveMechanism, privateKey, deriveAttributes);

        byte[] derivedValue = this.GetValue(session, derivedHandle);
    }

    [TestMethod]
    public void Derive_ECDH1WithCkdNull_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"EcPrivKey-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> privateAttrs = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_EC),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, false),

            factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, HexConvertor.GetBytes(P256_1_Ecparams)),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, HexConvertor.GetBytes(P256_1_EcValue)),
        };

        IObjectHandle privateKeyHandle = session.CreateObject(privateAttrs);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkEcdh1DeriveParams mp = factories.MechanismParamsFactory.CreateCkEcdh1DeriveParams((ulong)CKD.CKD_NULL,
                null,
                HexConvertor.GetBytes(P256_2_EcPoint));
        using IMechanism deriveMechanism = factories.MechanismFactory.Create(CKM.CKM_ECDH1_DERIVE, mp);

        List<IObjectAttribute> deriveAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, 32)
        };

        IObjectHandle derivedKey = session.DeriveKey(deriveMechanism, privateKeyHandle, deriveAttributes);

        byte[] derivedValue = this.GetValue(session, derivedKey);
        byte[] exceptedDerivedKey = HexConvertor.GetBytes(P256_1And2_DerivedNullValue);

        Assert.AreEqual(BitConverter.ToString(exceptedDerivedKey), BitConverter.ToString(derivedValue));
    }

    [DataTestMethod]
    [DataRow(CKD.CKD_SHA1_KDF, "16382A2748CE3A030107AAFF00A410")]
    [DataRow(CKD.CKD_SHA1_KDF, "")]
    [DataRow(CKD.CKD_SHA224_KDF, "02ab5394bc64060eca70550642af3a1a")]
    [DataRow(CKD.CKD_SHA256_KDF, "")]
    [DataRow(CKD.CKD_SHA256_KDF, "db4fb93a21bf23198fbef6a99d7eb9ee33b99a8055093a369e65d41e7367ccb5aacb762605b9bc9d4c29e721184bb12d411b95944d77363ee3f2db17")]
    [DataRow(CKD.CKD_SHA384_KDF, "AA00AA00")]
    [DataRow(CKD.CKD_SHA512_KDF, "1b649e91df70de27")]
    public void Derive_ECDH1WithSharedData_Success(CKD kdfFunction, string sharedData)
    {

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"EcPrivKey-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> privateAttrs = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_EC),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, false),

            factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, HexConvertor.GetBytes(P256_1_Ecparams)),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, HexConvertor.GetBytes(P256_1_EcValue)),
        };

        IObjectHandle privateKeyHandle = session.CreateObject(privateAttrs);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkEcdh1DeriveParams mp = factories.MechanismParamsFactory.CreateCkEcdh1DeriveParams((ulong)kdfFunction,
                (string.IsNullOrEmpty(sharedData)) ? null : HexConvertor.GetBytes(sharedData),
                HexConvertor.GetBytes(P256_2_EcPoint));
        using IMechanism deriveMechanism = factories.MechanismFactory.Create(CKM.CKM_ECDH1_DERIVE, mp);

        List<IObjectAttribute> deriveAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_GENERIC_SECRET),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, 32)
        };

        IObjectHandle derivedKey = session.DeriveKey(deriveMechanism, privateKeyHandle, deriveAttributes);

        _ = this.GetValue(session, derivedKey);
    }

    [TestMethod]
    public void Derive_ECDH1Sha1AesKey_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        string label = $"EcPrivKey-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        List<IObjectAttribute> privateAttrs = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_EC),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, false),

            factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, HexConvertor.GetBytes(P256_1_Ecparams)),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, HexConvertor.GetBytes(P256_1_EcValue)),
        };

        IObjectHandle privateKeyHandle = session.CreateObject(privateAttrs);

        using Net.Pkcs11Interop.HighLevelAPI.MechanismParams.ICkEcdh1DeriveParams mp = factories.MechanismParamsFactory.CreateCkEcdh1DeriveParams((ulong)CKD.CKD_SHA1_KDF,
                null,
                HexConvertor.GetBytes(P256_2_EcPoint));
        using IMechanism deriveMechanism = factories.MechanismFactory.Create(CKM.CKM_ECDH1_DERIVE, mp);

        List<IObjectAttribute> deriveAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_SECRET_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_AES),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_UNWRAP, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, 32)
        };

        IObjectHandle derivedKey = session.DeriveKey(deriveMechanism, privateKeyHandle, deriveAttributes);

        Assert.IsNotNull(derivedKey);
    }

    private byte[] GetValue(ISession session, IObjectHandle handle, CKA attr = CKA.CKA_VALUE)
    {
        return session.GetAttributeValue(handle, new List<CKA>() { attr })[0].GetValueAsByteArray();
    }

    private (IObjectHandle publicKey, IObjectHandle privateKey) GenerateEcKeypair(ISession session, byte[] ecParams)
    {
        string label = $"EcKeyPari-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = session.GenerateRandom(32);

        Pkcs11InteropFactories factories = session.Factories;
        List<IObjectAttribute> publicAttrs = new List<IObjectAttribute>()
        {
           factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
           factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
           factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
           factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
           factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
           factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
           factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY_RECOVER, false),
           factories.ObjectAttributeFactory.Create(CKA.CKA_WRAP, true),
           factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
           factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_EC),
           factories.ObjectAttributeFactory.Create(CKA.CKA_EC_PARAMS, ecParams),
        };

        List<IObjectAttribute> privateAttrs = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK.CKK_EC),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DECRYPT, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_DERIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN_RECOVER, false),
        };


        using IMechanism generateKeyPairMechnism = factories.MechanismFactory.Create(CKM.CKM_ECDSA_KEY_PAIR_GEN);
        session.GenerateKeyPair(generateKeyPairMechnism,
            publicAttrs,
            privateAttrs,
            out IObjectHandle publicKeyHandle,
            out IObjectHandle privateKeyHandle);

        return (publicKeyHandle, privateKeyHandle);
    }

    private byte[] ExtractEcPoint(ECDsa dotnetEcDsa)
    {
        ECParameters externEcParams = dotnetEcDsa.ExportParameters(false);
        Assert.IsTrue(externEcParams.Q.X != null);
        Assert.IsTrue(externEcParams.Q.Y != null);

        byte[] exParamsOctets = new byte[1 + externEcParams.Q.X.Length * 2];
        exParamsOctets[0] = 0x04;
        externEcParams.Q.X.CopyTo(exParamsOctets, 1);
        externEcParams.Q.Y.CopyTo(exParamsOctets, 1 + externEcParams.Q.X.Length);

        return (new Org.BouncyCastle.Asn1.DerOctetString(exParamsOctets)).GetEncoded();
    }
}