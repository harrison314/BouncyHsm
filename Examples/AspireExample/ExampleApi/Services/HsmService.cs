using Net.Pkcs11Interop.HighLevelAPI;

namespace ExampleApi.Services;
public class HsmService : IHsmService, IDisposable
{
    private bool disposedValue;
    private readonly IPkcs11Library library;

    public HsmService()
    {
        Pkcs11InteropFactories interopFactories = new Pkcs11InteropFactories();
        this.library = interopFactories.Pkcs11LibraryFactory.LoadPkcs11Library(interopFactories,
              BouncyHsm.Client.BouncyHsmPkcs11Paths.CurrentPlatformSpecificPkcs11Path,
              Net.Pkcs11Interop.Common.AppType.MultiThreaded);
    }

    public List<TokenInfo> GetSlots()
    {
        List<ISlot> slots = this.library.GetSlotList(Net.Pkcs11Interop.Common.SlotsType.WithTokenPresent);
        return slots.Select(t => t.GetTokenInfo())
              .Select(t => new TokenInfo(t.SlotId, t.Label, t.SerialNumber))
              .ToList();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.library.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            this.disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
