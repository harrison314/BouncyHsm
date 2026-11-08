using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Pkcs11Interop.Ext.Common;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T20_SignHss
{
    private const int Sig1LevelH5Length = 1296;

    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void Sign_Success_LengthAndRemaining()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = LoadLibrary(factories);
        using ISession session = OpenLoggedSession(library);

        CreateHssKeyPair(factories, session, 1, new ulong[] { CK_LMS_TYPE.LMS_SHA256_M32_H5 }, new ulong[] { CK_LMOTS_TYPE.LMOTS_SHA256_N32_W8 }, true, out _, out IObjectHandle privateKey);

        Assert.AreEqual(32UL, GetRemaining(session, privateKey));

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_2.CKM_HSS);
        byte[] signature = session.Sign(mechanism, privateKey, RandomNumberGenerator.GetBytes(48));

        Assert.AreEqual(Sig1LevelH5Length, signature.Length);
        Assert.AreEqual(31UL, GetRemaining(session, privateKey));
    }

    [TestMethod]
    public void Sign_LengthQueryThenSign_ConsumesOne()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = LoadLibrary(factories);
        using ISession session = OpenLoggedSession(library);

        CreateHssKeyPair(factories, session, 1, new ulong[] { CK_LMS_TYPE.LMS_SHA256_M32_H5 }, new ulong[] { CK_LMOTS_TYPE.LMOTS_SHA256_N32_W8 }, true, out _, out IObjectHandle privateKey);

        if (Platform.NativeULongSize != 8)
        {
            Assert.Inconclusive("Test requires 64-bit CK_ULONG.");
        }

        IntPtr libHandle = NativeLibrary.Load(AssemblyTestConstants.P11LibPath);
        try
        {
            this.LengthQueryThenSign(session, privateKey, libHandle);
        }
        finally
        {
            NativeLibrary.Free(libHandle);
        }
    }

    private delegate ulong C_SignInitDelegate(ulong hSession, ref CkMechanism mechanism, ulong hKey);

    private delegate ulong C_SignDelegate(ulong hSession, byte[] data, ulong dataLen, byte[]? signature, ref ulong signatureLen);

    [StructLayout(LayoutKind.Sequential)]
    private struct CkMechanism
    {
        public ulong Mechanism;
        public IntPtr Parameter;
        public ulong ParameterLen;
    }

    private void LengthQueryThenSign(ISession session, IObjectHandle privateKey, IntPtr libHandle)
    {
        C_SignInitDelegate signInit = Marshal.GetDelegateForFunctionPointer<C_SignInitDelegate>(NativeLibrary.GetExport(libHandle, "C_SignInit"));
        C_SignDelegate sign = Marshal.GetDelegateForFunctionPointer<C_SignDelegate>(NativeLibrary.GetExport(libHandle, "C_Sign"));
        ulong sessionId = session.SessionId;
        ulong keyId = privateKey.ObjectId;

        CkMechanism mechanism = new CkMechanism
        {
            Mechanism = (ulong)CKM_V3_2.CKM_HSS,
            Parameter = IntPtr.Zero,
            ParameterLen = 0
        };
        byte[] data = RandomNumberGenerator.GetBytes(48);

        Assert.AreEqual((ulong)CKR.CKR_OK, signInit(sessionId, ref mechanism, keyId));

        ulong len = 0;
        Assert.AreEqual((ulong)CKR.CKR_OK, sign(sessionId, data, (ulong)data.Length, null, ref len));
        Assert.AreEqual((ulong)Sig1LevelH5Length, len);

        ulong smallLen = 10;
        ulong rv = sign(sessionId, data, (ulong)data.Length, new byte[10], ref smallLen);
        Assert.AreEqual((ulong)CKR.CKR_BUFFER_TOO_SMALL, rv);
        Assert.AreEqual((ulong)Sig1LevelH5Length, smallLen);

        byte[] signature = new byte[Sig1LevelH5Length];
        ulong sigLen = (ulong)signature.Length;
        Assert.AreEqual((ulong)CKR.CKR_OK, sign(sessionId, data, (ulong)data.Length, signature, ref sigLen));
        Assert.AreEqual((ulong)Sig1LevelH5Length, sigLen);

        Assert.AreEqual(31UL, GetRemaining(session, privateKey));
    }

    [TestMethod]
    public void Sign_UntilExhausted_KeyExhausted()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = LoadLibrary(factories);
        using ISession session = OpenLoggedSession(library);

        CreateHssKeyPair(factories, session, 1, new ulong[] { CK_LMS_TYPE.LMS_SHA256_M32_H5 }, new ulong[] { CK_LMOTS_TYPE.LMOTS_SHA256_N32_W8 }, true, out _, out IObjectHandle privateKey);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_2.CKM_HSS);
        byte[] data = RandomNumberGenerator.GetBytes(48);

        long previous = -1;
        for (int i = 0; i < 32; i++)
        {
            byte[] signature = session.Sign(mechanism, privateKey, data);
            uint index = LeafIndex(signature);
            Assert.AreEqual((uint)i, index, "Unexpected leaf index.");
            Assert.IsGreaterThan(previous, (long)index);
            previous = index;
        }

        Pkcs11Exception ex = Assert.Throws<Pkcs11Exception>(() => session.Sign(mechanism, privateKey, data));
        Assert.AreEqual(CKR_V3_1.CKR_KEY_EXHAUSTED, ex.RV);

        // The 33rd attempt fails already in C_SignInit (remaining-count check in HssWrapperSigner),
        // so no sign operation is left active; the remaining count is read on a separate session.
        using ISession otherSession = OpenLoggedSession(library);
        Assert.AreEqual(0UL, GetRemaining(otherSession, privateKey));
    }

    [TestMethod]
    public void Sign_TwoSessions_Interleaved_DistinctIndexes()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = LoadLibrary(factories);
        using ISession session1 = OpenLoggedSession(library);
        using ISession session2 = library.GetSlotList(SlotsType.WithTokenPresent).SelectTestSlot().OpenSession(SessionType.ReadWrite);

        CreateHssKeyPair(factories, session1, 1, new ulong[] { CK_LMS_TYPE.LMS_SHA256_M32_H5 }, new ulong[] { CK_LMOTS_TYPE.LMOTS_SHA256_N32_W8 }, true, out _, out IObjectHandle privateKey);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_2.CKM_HSS);
        byte[] data = RandomNumberGenerator.GetBytes(48);

        HashSet<uint> indexes = new HashSet<uint>();
        for (int i = 0; i < 6; i++)
        {
            ISession s = i % 2 == 0 ? session1 : session2;
            byte[] signature = s.Sign(mechanism, privateKey, data);
            Assert.IsTrue(indexes.Add(LeafIndex(signature)), "Leaf index was used twice.");
        }

        Assert.HasCount(6, indexes);
        Assert.AreEqual(26UL, GetRemaining(session1, privateKey));
    }

    [TestMethod]
    public void Sign_AfterLibraryReload_StateKept()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        byte[] ckId = Utils.GetRandomBytes(32, true);

        using (IPkcs11Library library = LoadLibrary(factories))
        {
            using ISession session = OpenLoggedSession(library);
            CreateHssKeyPair(factories, session, 1, new ulong[] { CK_LMS_TYPE.LMS_SHA256_M32_H5 }, new ulong[] { CK_LMOTS_TYPE.LMOTS_SHA256_N32_W8 }, true, out _, out IObjectHandle privateKey, ckId);

            using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_2.CKM_HSS);
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual((uint)i, LeafIndex(session.Sign(mechanism, privateKey, RandomNumberGenerator.GetBytes(48))));
            }
        }

        using (IPkcs11Library library = LoadLibrary(factories))
        {
            using ISession session = OpenLoggedSession(library);
            List<IObjectHandle> found = session.FindAllObjects(new List<IObjectAttribute>()
            {
                factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY),
                factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK_V3_0.CKK_HSS),
                factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            });
            Assert.HasCount(1, found);

            Assert.AreEqual(29UL, GetRemaining(session, found[0]));

            using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_2.CKM_HSS);
            Assert.AreEqual(3U, LeafIndex(session.Sign(mechanism, found[0], RandomNumberGenerator.GetBytes(48))));
        }
    }

    [TestMethod]
    public void Sign_SessionKey_DistinctIndexes()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = LoadLibrary(factories);
        using ISession session = OpenLoggedSession(library);

        CreateHssKeyPair(factories, session, 1, new ulong[] { CK_LMS_TYPE.LMS_SHA256_M32_H5 }, new ulong[] { CK_LMOTS_TYPE.LMOTS_SHA256_N32_W8 }, false, out _, out IObjectHandle privateKey);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_2.CKM_HSS);
        Assert.AreEqual(0U, LeafIndex(session.Sign(mechanism, privateKey, RandomNumberGenerator.GetBytes(48))));
        Assert.AreEqual(1U, LeafIndex(session.Sign(mechanism, privateKey, RandomNumberGenerator.GetBytes(48))));
        Assert.AreEqual(30UL, GetRemaining(session, privateKey));
    }

    [TestMethod]
    public void SignUpdate_FunctionNotSupported()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = LoadLibrary(factories);
        using ISession session = OpenLoggedSession(library);

        CreateHssKeyPair(factories, session, 1, new ulong[] { CK_LMS_TYPE.LMS_SHA256_M32_H5 }, new ulong[] { CK_LMOTS_TYPE.LMOTS_SHA256_N32_W8 }, false, out _, out IObjectHandle privateKey);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_2.CKM_HSS);
        using MemoryStream input = new MemoryStream(RandomNumberGenerator.GetBytes(48));
        Pkcs11Exception ex = Assert.Throws<Pkcs11Exception>(() => session.Sign(mechanism, privateKey, input));
        Assert.AreEqual(CKR.CKR_FUNCTION_NOT_SUPPORTED, ex.RV);
    }

    [TestMethod]
    public void ImportedPrivateKey_SignsFromImportedIndex()
    {
        // The private key value can not be exported through PKCS#11 (sensitive), so it is built with BouncyCastle.
        Org.BouncyCastle.Pqc.Crypto.Lms.HssKeyPairGenerator generator = new Org.BouncyCastle.Pqc.Crypto.Lms.HssKeyPairGenerator();
        generator.Init(new Org.BouncyCastle.Pqc.Crypto.Lms.HssKeyGenerationParameters(
            new[]
            {
                new Org.BouncyCastle.Pqc.Crypto.Lms.LmsParameters(Org.BouncyCastle.Pqc.Crypto.Lms.LMSigParameters.lms_sha256_n32_h5,
                    Org.BouncyCastle.Pqc.Crypto.Lms.LMOtsParameters.sha256_n32_w8)
            },
            new Org.BouncyCastle.Security.SecureRandom()));
        Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair keyPair = generator.GenerateKeyPair();
        Org.BouncyCastle.Pqc.Crypto.Lms.HssPrivateKeyParameters bcPrivateKey = (Org.BouncyCastle.Pqc.Crypto.Lms.HssPrivateKeyParameters)keyPair.Private;
        Org.BouncyCastle.Pqc.Crypto.Lms.HssPublicKeyParameters bcPublicKey = (Org.BouncyCastle.Pqc.Crypto.Lms.HssPublicKeyParameters)keyPair.Public;

        // Consume leaf 0 outside of the HSM, so the imported key starts at index 1.
        Org.BouncyCastle.Pqc.Crypto.Lms.HssSigner bcSigner = new Org.BouncyCastle.Pqc.Crypto.Lms.HssSigner();
        bcSigner.Init(true, bcPrivateKey);
        _ = bcSigner.GenerateSignature(RandomNumberGenerator.GetBytes(16));

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = LoadLibrary(factories);
        using ISession session = OpenLoggedSession(library);

        string label = $"HssImport-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32, true);

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK_V3_0.CKK_HSS),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_HSS_LEVELS, 1UL),
            factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_HSS_LMS_TYPES, ToBytes(new ulong[] { CK_LMS_TYPE.LMS_SHA256_M32_H5 })),
            factories.ObjectAttributeFactory.Create(CKA_V3_2.CKA_HSS_LMOTS_TYPES, ToBytes(new ulong[] { CK_LMOTS_TYPE.LMOTS_SHA256_N32_W8 })),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, bcPrivateKey.GetEncoded()),
        };

        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
            factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PUBLIC_KEY),
            factories.ObjectAttributeFactory.Create(CKA.CKA_KEY_TYPE, CKK_V3_0.CKK_HSS),
            factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE, bcPublicKey.GetEncoded()),
        };

        IObjectHandle privateKey = session.CreateObject(privateKeyAttributes);
        IObjectHandle publicKey = session.CreateObject(publicKeyAttributes);

        Assert.AreEqual(31UL, GetRemaining(session, privateKey));

        byte[] dataToSign = RandomNumberGenerator.GetBytes(48);
        using IMechanism mechanism = factories.MechanismFactory.Create(CKM_V3_2.CKM_HSS);
        byte[] signature = session.Sign(mechanism, privateKey, dataToSign);

        Assert.AreEqual(1U, LeafIndex(signature));

        session.Verify(mechanism, publicKey, dataToSign, signature, out bool isValid);
        Assert.IsTrue(isValid, "Signature is not valid.");

        Assert.AreEqual(30UL, GetRemaining(session, privateKey));
    }

    public static uint LeafIndex(byte[] signature)
    {
        return BinaryPrimitives.ReadUInt32BigEndian(signature.AsSpan(4, 4));
    }

    public static ulong GetRemaining(ISession session, IObjectHandle privateKey)
    {
        List<IObjectAttribute> attributes = session.GetAttributeValue(privateKey, new List<CKA>() { CKA_V3_2.CKA_HSS_KEYS_REMAINING });
        return attributes[0].GetValueAsUlong();
    }

    public static IPkcs11Library LoadLibrary(Pkcs11InteropFactories factories)
    {
        return factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);
    }

    public static ISession OpenLoggedSession(IPkcs11Library library)
    {
        ISlot slot = library.GetSlotList(SlotsType.WithTokenPresent).SelectTestSlot();
        ISession session = slot.OpenSession(SessionType.ReadWrite);
        try
        {
            session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);
        }
        catch (Pkcs11Exception ex) when (ex.RV == CKR.CKR_USER_ALREADY_LOGGED_IN)
        {
        }

        return session;
    }

    public static void CreateHssKeyPair(Pkcs11InteropFactories f,
        ISession s,
        ulong levels,
        ulong[] lmsTypes,
        ulong[] lmotsTypes,
        bool token,
        out IObjectHandle pub,
        out IObjectHandle priv,
        byte[]? ckId = null,
        bool extractable = false)
    {
        string label = $"HssTest-{DateTime.UtcNow}-{RandomNumberGenerator.GetInt32(100, 999)}";
        ckId ??= Utils.GetRandomBytes(32, true);

        List<IObjectAttribute> publicKeyAttributes = new List<IObjectAttribute>()
        {
            f.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, token),
            f.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, false),
            f.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            f.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            f.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
        };

        List<IObjectAttribute> privateKeyAttributes = new List<IObjectAttribute>()
        {
            f.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, token),
            f.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            f.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            f.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            f.ObjectAttributeFactory.Create(CKA.CKA_SIGN, true),
            f.ObjectAttributeFactory.Create(CKA_V3_2.CKA_HSS_LEVELS, levels),
            f.ObjectAttributeFactory.Create(CKA_V3_2.CKA_HSS_LMS_TYPES, ToBytes(lmsTypes)),
            f.ObjectAttributeFactory.Create(CKA_V3_2.CKA_HSS_LMOTS_TYPES, ToBytes(lmotsTypes)),
        };

        if (extractable)
        {
            privateKeyAttributes.Add(f.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, true));
        }

        using IMechanism mechanism = f.MechanismFactory.Create(CKM_V3_2.CKM_HSS_KEY_PAIR_GEN);
        s.GenerateKeyPair(mechanism, publicKeyAttributes, privateKeyAttributes, out pub, out priv);
    }

    // CK_ULONG array attribute value: native-endian items of the platform CK_ULONG size.
    private static byte[] ToBytes(ulong[] values)
    {
        int size = Platform.NativeULongSize;
        byte[] result = new byte[values.Length * size];
        for (int i = 0; i < values.Length; i++)
        {
            Span<byte> item = result.AsSpan(i * size, size);
            if (size == sizeof(ulong))
            {
                BitConverter.TryWriteBytes(item, values[i]);
            }
            else
            {
                BitConverter.TryWriteBytes(item, (uint)values[i]);
            }
        }

        return result;
    }
}
