namespace BouncyHsm.Infrastructure.Storage.LiteDbFile.DbModels;

public class PasswordsModel
{
    public Guid Id
    {
        get;
        set;
    }

    public Pbkdf2PasswordModel UserPin
    {
        get;
        set;
    }

    public Pbkdf2PasswordModel SoPin
    {
        get;
        set;
    }

    public Pbkdf2PasswordModel? SignaturePin
    {
        get;
        set;
    }

    public PasswordsModel()
    {
        this.UserPin = default!;
        this.SoPin = default!;
    }
}
