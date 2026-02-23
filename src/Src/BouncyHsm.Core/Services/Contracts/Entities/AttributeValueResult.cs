using System.Diagnostics.CodeAnalysis;

namespace BouncyHsm.Core.Services.Contracts.Entities;

[Dunet.Union]
public partial record AttributeValueResult
{
    partial record Ok(IAttributeValue Value);
    partial record Computed(Task<IAttributeValue> TaskValue);
    partial record SensitiveOrUnextractable();
    partial record InvalidAttribute();

    //public bool IsOK([NotNullWhen(true)] out IAttributeValue? value)
    //{
    //    (value, bool match) = this.MatchOk<(IAttributeValue?, bool)>(ok => (ok.Value, true),
    //        () => (null, false));

    //    return match;
    //}

    public async Task<(bool IsOk, IAttributeValue? Value)> GetOkOrComputed()
    {
        (bool isOk, Task<IAttributeValue>? task) = this.Match<(bool IsOk, Task<IAttributeValue>? Value)>(ok => (true, Task.FromResult(ok.Value)),
              computed => (true, computed.TaskValue),
              _ => (false, null),
              _ => (false, null));

        if (isOk)
        {
            return (true, await task!);
        }

        return (false, null);
    }

    public sealed override string ToString()
    {
        return this.Match(ok => $"Value {ok.Value}",
            computed => "Computed attribute.",
            sensitiveOrUnextractable => "Sensitive or unextractable attribute.",
            invalidAttribute => "Attribute is invalid for object.");
    }
}