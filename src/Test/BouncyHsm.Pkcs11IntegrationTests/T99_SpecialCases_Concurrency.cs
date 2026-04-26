using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T99_SpecialCases_Concurrency
{
    private const int ThreadCount = 8;

    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void Run_Concurrent_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();
        this.InitToken(slot);

        Thread[] threads = new Thread[ThreadCount];
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(this.Worker)
            {
                IsBackground = true
            };
        }

        using ISession masterSession = slot.OpenSession(SessionType.ReadOnly);
        masterSession.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        TestState testState = new TestState(slot, TimeSpan.FromSeconds(10.0));
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i].Start(testState);
        }

        for (int i = 0; i < threads.Length; i++)
        {
            threads[i].Join();
        }

        Assert.AreEqual(0, testState.Failed, "Concurency access failed.");
    }

    private void InitToken(ISlot slot)
    {
        slot.InitToken(AssemblyTestConstants.SoPin, "TestLabel1");
        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.InitPin("Foobar");
        session.InitPin(AssemblyTestConstants.UserPin);
    }

    private void Worker(object? state)
    {
        TestState testState = (TestState)state!;

        while (!testState.CancellationToken.IsCancellationRequested)
        {
            try
            {
                using ISession session = testState.Slot.OpenSession(SessionType.ReadWrite);
                List<IObjectAttribute> searchAttributes = new List<IObjectAttribute>()
                {
                    session.Factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PUBLIC_KEY),
                };

                _ = session.FindAllObjects(searchAttributes);

                _ = testState.Slot.GetSlotInfo();
                testState.IncrementSuccess();
            }
            catch (Pkcs11Exception ex)
            {
                this.TestContext?.WriteLine("Error {0} with message: {1}", ex.RV, ex.Message);
                testState.IncrementError();
            }
        }
    }

    private class TestState
    {
        private long success;
        private long failed;
        private CancellationTokenSource cts;

        public ISlot Slot
        {
            get;
        }

        public long Success
        {
            get => this.success;
        }

        public long Failed
        {
            get => this.failed;
        }

        public CancellationToken CancellationToken
        {
            get => this.cts.Token;
        }

        public TestState(ISlot slot, TimeSpan delay)
        {
            this.Slot = slot;
            this.success = 0;
            this.failed = 0;
            this.cts = new CancellationTokenSource(delay);
        }

        public void IncrementSuccess()
        {
            Interlocked.Increment(ref this.success);
        }

        public void IncrementError()
        {
            Interlocked.Increment(ref this.failed);
        }
    }
}
