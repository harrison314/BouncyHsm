using Org.BouncyCastle.OpenSsl;

namespace BouncyHsm.Core.UseCases.Implementation.Generators;

internal class PemPasswordFinder : IPasswordFinder
{
    private char[]? password;

    public PemPasswordFinder(char[]? password)
    {
        this.password = password;
    }

    public char[] GetPassword()
    {
        return this.password!; //Damit operator for BC interface
    }
}