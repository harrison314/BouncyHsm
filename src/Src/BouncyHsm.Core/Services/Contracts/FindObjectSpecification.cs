using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.Contracts;

public class FindObjectSpecification : ISpecification
{
    public IReadOnlyDictionary<CKA, IAttributeValue> Template
    {
        get;
    }

    public bool IsUserLogged
    {
        get;
    }

    public FindObjectSpecification(IReadOnlyDictionary<CKA, IAttributeValue> template, bool isUserLogged)
    {
        this.Template = template;
        this.IsUserLogged = isUserLogged;
    }
}