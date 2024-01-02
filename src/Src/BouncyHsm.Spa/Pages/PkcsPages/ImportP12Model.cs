using BouncyHsm.Client;
using BouncyHsm.Spa.Shared;

namespace BouncyHsm.Spa.Pages.Pkcs;

public class ImportP12Model
{
    public string CkaLabel
    {
        get;
        set;
    }

    public string CkaIdText
    {
        get;
        set;
    }

    public BinaryForm CkaIdForm
    {
        get;
        set;
    }

    public P12ImportMode ImportMode
    {
        get;
        set;
    }

    public bool ImportChain
    {
        get;
        set;
    }

    public string Password
    {
        get;
        set;
    }

    public ImportP12Model()
    {
        this.CkaLabel = string.Empty;
        this.CkaIdText = string.Empty;
        this.Password = string.Empty;

        this.CkaIdForm = BinaryForm.Utf8;
        this.ImportMode = P12ImportMode.Imported;
        this.ImportChain = false;
    }
}
