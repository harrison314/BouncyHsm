using System.Diagnostics.CodeAnalysis;

namespace BouncyHsm.Core.Services.Contracts.Entities;

[Dunet.Union]
public partial record AttributeValueResult
{
    partial record Ok(IAttributeValue Value);
    partial record SensitiveOrUnextractable();
    partial record InvalidAttribute();

    public bool IsOK([NotNullWhen(true)] out IAttributeValue? value)
    {
        (value, bool match) = this.MatchOk<(IAttributeValue?, bool)>(ok => (ok.Value, true), 
            () => (null, false));
        
        return match;
    }

    public sealed override string ToString()
    {
        return this.Match(ok => $"Value {ok.Value}",
            sensitiveOrUnextractable => "Sensitive or unextractable attribute.",
            invalidAttribute => "Attribute is invalid for object.");
    }
}