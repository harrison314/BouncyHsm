using BouncyHsm.Client;
using BouncyHsm.Spa.Shared;
using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Spa.Pages.Pkcs;

public class ImportPemModel
{
    [Required]
    public string Pem
    {
        get;
        set;
    }

    [Required]
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

    public PrivateKeyImportMode ImportMode
    {
        get;
        set;
    }

    public string Password
    {
        get;
        set;
    }

    [Required]
    public bool ForSigning
    {
        get;
        set;
    }

    [Required]
    public bool ForEncryption
    {
        get;
        set;
    }

    [Required]
    public bool ForDerivation
    {
        get;
        set;
    }

    [Required]
    public bool ForWrap
    {
        get;
        set;
    }

    public ImportPemModel()
    {
        this.Pem = string.Empty;
        this.CkaLabel = string.Empty;
        this.CkaIdText = string.Empty;
        this.Password = string.Empty;

        this.CkaIdForm = BinaryForm.Utf8;
        this.ImportMode = PrivateKeyImportMode.Imported;
    }
}
