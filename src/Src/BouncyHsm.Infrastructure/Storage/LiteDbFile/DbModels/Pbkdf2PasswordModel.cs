namespace BouncyHsm.Infrastructure.Storage.LiteDbFile.DbModels;

public class Pbkdf2PasswordModel
{
    public int Iterations
    {
        get;
        set;
    }

    public byte[] Salt
    {
        get;
        set;
    }

    public byte[] Hash
    {
        get;
        set;
    }

    public Pbkdf2PasswordModel()
    {
        this.Salt = Array.Empty<byte>();
        this.Hash = Array.Empty<byte>();
    }

    public Pbkdf2PasswordModel(int iterations, byte[] salt, byte[] hash)
    {
        this.Iterations = iterations;
        this.Salt = salt;
        this.Hash = hash;
    }
}