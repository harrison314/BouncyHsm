namespace BouncyHsm.Core.UseCases.Contracts;

public class PkcsObjects
{
    public IReadOnlyList<PkcsObjectInfo> Objects
    {
        get;
        set;
    }

    public PkcsObjects(IReadOnlyList<PkcsObjectInfo> objects)
    {
        this.Objects = objects;
    }
}
