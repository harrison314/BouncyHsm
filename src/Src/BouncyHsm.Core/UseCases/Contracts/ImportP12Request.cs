namespace BouncyHsm.Core.UseCases.Contracts;

public class ImportP12Request
{
    public uint SlotId
    {
        get;
        set;
    }

    public string CkaLabel
    {
        get;
        set;
    }

    public byte[] CkaId
    {
        get;
        set;
    }

    public PrivateKeyImportMode ImportMode
    {
        get;
        set;
    }

    public bool ImportChain
    {
        get;
        set;
    }

    public byte[] Pkcs12Content
    {
        get;
        set;
    }

    public string Password
    {
        get;
        set;
    }

    public ImportP12Request()
    {
        this.CkaLabel = string.Empty;
        this.CkaId = Array.Empty<byte>();
        this.Pkcs12Content = Array.Empty<byte>();
        this.Password = string.Empty;
    }
}