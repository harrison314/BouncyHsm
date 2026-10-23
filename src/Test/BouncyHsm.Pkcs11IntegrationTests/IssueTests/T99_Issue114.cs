using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Pkcs11Interop.Ext;
using System.Runtime.InteropServices;

namespace BouncyHsm.Pkcs11IntegrationTests.IssueTests;

[TestClass]
[TestCategory(IssueTestConstants.Cathegory)]
[Ignore]
public class T99_Issue114
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    private delegate uint C_EncryptInit_4(uint hSession, IntPtr pMechanism, uint hKey);
    private delegate ulong C_EncryptInit_8(ulong hSession, IntPtr pMechanism, ulong hKey);

    private delegate uint C_Encrypt_4(uint hSession, IntPtr pData, uint ulDataLen, IntPtr pEncryptedData, ref uint pulEncryptedDataLen);
    private delegate ulong C_Encrypt_8(ulong hSession, IntPtr pData, ulong ulDataLen, IntPtr pEncryptedData, ref ulong pulEncryptedDataLen);


    [TestMethod]
    public void Encrypt_BufferTooSmall4_Success()
    {
        if (Platform.NativeULongSize != 4)
        {
            Assert.Inconclusive($"Invalid NativeULongSize");
        }

        byte[] plainText = new byte[16];
        Random.Shared.NextBytes(plainText);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle key = this.GenerateAesKey(session, 32);
        byte[] iv = Utils.GetRandomBytes(16);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_CBC, iv);

        IntPtr libHandle = this.GetNativeLibHandle(library);
        C_EncryptInit_4 ectyptInit = this.GetDelegate<C_EncryptInit_4>(libHandle, "C_EncryptInit");
        C_Encrypt_4 ectypt = this.GetDelegate<C_Encrypt_4>(libHandle, "C_Encrypt");

        IntPtr mechanismPtr = Marshal.AllocHGlobal(Marshal.SizeOf(mechanism.ToMarshalableStructure()));
        Marshal.StructureToPtr(mechanism.ToMarshalableStructure(), mechanismPtr, false);
        CKR initCkr = (CKR)ectyptInit((uint)session.SessionId, mechanismPtr, (uint)key.ObjectId);
        Marshal.FreeHGlobal(mechanismPtr);
        Assert.AreEqual(CKR.CKR_OK, initCkr);

        IntPtr plaintextPtr = Marshal.AllocHGlobal(plainText.Length);
        Marshal.Copy(plainText, 0, plaintextPtr, plainText.Length);

        IntPtr encryptedPtr = Marshal.AllocHGlobal(5);
        uint encryptedLen = 5;


        CKR encryptCkr = (CKR)ectypt((uint)session.SessionId,
            plaintextPtr, (uint)plainText.Length,
            encryptedPtr, ref encryptedLen);

        Marshal.FreeHGlobal(plaintextPtr);
        Marshal.FreeHGlobal(encryptedPtr);

        Assert.AreEqual(CKR.CKR_BUFFER_TOO_SMALL, initCkr);
        Assert.AreEqual((uint)16, encryptedLen);
    }

    [TestMethod]
    public void Encrypt_BufferTooSmall8_Success()
    {
        if (Platform.NativeULongSize != 8)
        {
            Assert.Inconclusive($"Invalid NativeULongSize");
        }

        byte[] plainText = new byte[16];
        Random.Shared.NextBytes(plainText);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        IObjectHandle key = this.GenerateAesKey(session, 32);
        byte[] iv = Utils.GetRandomBytes(16);

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_CBC, iv);


        IntPtr libHandle = this.GetNativeLibHandle(library);
        C_EncryptInit_8 ectyptInit = this.GetDelegate<C_EncryptInit_8>(libHandle, "C_EncryptInit");
        C_Encrypt_8 ectypt = this.GetDelegate<C_Encrypt_8>(libHandle, "C_Encrypt");

        IntPtr mechanismPtr = Marshal.AllocHGlobal(Marshal.SizeOf(mechanism.ToMarshalableStructure()));
        Marshal.StructureToPtr(mechanism.ToMarshalableStructure(), mechanismPtr, false);
        CKR initCkr = (CKR)ectyptInit((ulong)session.SessionId, mechanismPtr, (ulong)key.ObjectId);
        Marshal.FreeHGlobal(mechanismPtr);
        Assert.AreEqual(CKR.CKR_OK, initCkr);

        IntPtr plaintextPtr = Marshal.AllocHGlobal(plainText.Length);
        Marshal.Copy(plainText, 0, plaintextPtr, plainText.Length);

        IntPtr encryptedPtr = Marshal.AllocHGlobal(5);
        ulong encryptedLen = 5;


        CKR encryptCkr = (CKR)ectypt((ulong)session.SessionId,
            plaintextPtr, (ulong)plainText.Length,
            encryptedPtr, ref encryptedLen);

        Marshal.FreeHGlobal(plaintextPtr);
        Marshal.FreeHGlobal(encryptedPtr);

        Assert.AreEqual(CKR.CKR_BUFFER_TOO_SMALL, initCkr);
        Assert.AreEqual((uint)16, encryptedLen);
    }

    public IObjectHandle GenerateAesKey(ISession session, int size)
    {
        string label = $"AES-{DateTime.UtcNow}-{Random.Shared.Next(100, 999)}";
        byte[] ckId = Utils.GetRandomBytes(32, true);

        List<IObjectAttribute> keyAttributes = new List<IObjectAttribute>()
        {
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_TOKEN, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_PRIVATE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_LABEL, label),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ID, ckId),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_ENCRYPT, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VERIFY, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_SENSITIVE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_EXTRACTABLE, false),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_DESTROYABLE, true),
            session.Factories.ObjectAttributeFactory.Create(CKA.CKA_VALUE_LEN, (uint)size),
        };

        using IMechanism mechanism = session.Factories.MechanismFactory.Create(CKM.CKM_AES_KEY_GEN);

        return session.GenerateKey(mechanism, keyAttributes);
    }

    private IntPtr GetNativeLibHandle(IPkcs11Library library)
    {
        System.Reflection.FieldInfo? pkcs11FieldInfo = library.GetType().GetField("_pkcs11Library", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (pkcs11FieldInfo == null)
        {
            throw new InvalidOperationException("Can not get _pkcs11Library field info");
        }

        object? pkcs11 = pkcs11FieldInfo.GetValue(library);
        if (pkcs11 == null)
        {
            throw new InvalidOperationException("Can not get _pkcs11Library value");
        }

        System.Reflection.FieldInfo? libHandleFieldInfo = pkcs11.GetType().GetField("_libraryHandle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (libHandleFieldInfo == null)
        {
            throw new InvalidOperationException("Can not get _libraryHandle field info");
        }

        object? libHandle = libHandleFieldInfo.GetValue(pkcs11);
        if (libHandle == null)
        {
            throw new InvalidOperationException("Can not get _libraryHandle value");
        }

        return (IntPtr)libHandle;
    }

    private TDelegate GetDelegate<TDelegate>(IntPtr lirarayhandle, string functionName)
       where TDelegate : Delegate
    {
        IntPtr procAddress = NativeLibrary.GetExport(lirarayhandle, functionName);
        if (procAddress == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Can not get address of {functionName} function");
        }
        return Marshal.GetDelegateForFunctionPointer<TDelegate>(procAddress);
    }
}
