using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Moq;

namespace BouncyHsm.Core.Tests.Services.P11Handlers.States;

[TestClass]
public class SignStateStatefulTests
{
    // IStatefulSigner is internal and Moq cannot proxy internal types of BouncyHsm.Core
    // (no InternalsVisibleTo for DynamicProxyGenAssembly2), so a recording fake is used.
    private sealed class FakeStatefulSigner : IStatefulSigner
    {
        private readonly byte[] result;

        public List<byte[]> Calls
        {
            get;
        } = new List<byte[]>();

        public uint SignatureSize
        {
            get => (uint)this.result.Length;
        }

        public FakeStatefulSigner(byte[] result)
        {
            this.result = result;
        }

        public ValueTask<byte[]> Sign(byte[] data, CancellationToken cancellationToken)
        {
            this.Calls.Add(data);
            return new ValueTask<byte[]>(this.result);
        }
    }

    [TestMethod]
    public async Task GetSignatureAsync_CalledTwice_SignsOnce()
    {
        byte[] expected = new byte[] { 0xAA, 0xBB };
        FakeStatefulSigner statefulSigner = new FakeStatefulSigner(expected);

        SignState state = new SignStateWithStatefulSigner(statefulSigner, Guid.NewGuid(), false, false);
        Assert.AreEqual((uint)expected.Length, state.GetMaxSignatureSize());
        Assert.AreEqual(0, statefulSigner.Calls.Count, "Length query must not sign.");
        state.Update(new byte[] { 1, 2 });
        state.Update(new byte[] { 3 });

        byte[] first = await state.GetSignatureAsync(CancellationToken.None);
        Assert.ThrowsExactly<BouncyHsm.Core.Services.Contracts.RpcPkcs11Exception>(() => state.Update(new byte[] { 4, 5, 6 }));
        byte[] second = await state.GetSignatureAsync(CancellationToken.None);

        Assert.AreSame(first, second);
        Assert.AreSame(expected, first);
        Assert.AreEqual(1, statefulSigner.Calls.Count);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, statefulSigner.Calls[0]);
    }

    [TestMethod]
    public async Task GetSignatureAsync_EmptyData_Allowed()
    {
        FakeStatefulSigner statefulSigner = new FakeStatefulSigner(new byte[] { 1 });

        SignState state = new SignStateWithStatefulSigner(statefulSigner, Guid.NewGuid(), false, false);
        state.Update(Array.Empty<byte>());

        _ = await state.GetSignatureAsync(CancellationToken.None);

        Assert.AreEqual(1, statefulSigner.Calls.Count);
        Assert.AreEqual(0, statefulSigner.Calls[0].Length);
    }

    [TestMethod]
    public void IsSinglePartOnly_TrueForStatefulState()
    {
        SignState state = new SignStateWithStatefulSigner(new FakeStatefulSigner(new byte[] { 1 }), Guid.NewGuid(), false, false);

        Assert.IsTrue(state.IsSinglePartOnly);
    }

    [TestMethod]
    public void IsSinglePartOnly_FalseForSignerState()
    {
        Mock<Org.BouncyCastle.Crypto.ISigner> signer = new Mock<Org.BouncyCastle.Crypto.ISigner>();

        SignState state = new SignStateWithSigner(signer.Object, Guid.NewGuid(), false, false, false);

        Assert.IsFalse(state.IsSinglePartOnly);
    }
}
