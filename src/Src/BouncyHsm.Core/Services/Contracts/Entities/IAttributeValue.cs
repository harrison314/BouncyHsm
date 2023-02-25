using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public interface IAttributeValue : IEquatable<IAttributeValue>, IEquatable<uint>
{
    AttrTypeTag TypeTag
    {
        get;
    }

    bool AsBool();

    string AsString();


    byte[] AsByteArray();

    uint AsUint();

    CkDate AsDate();

    uint GuessSize();
}
