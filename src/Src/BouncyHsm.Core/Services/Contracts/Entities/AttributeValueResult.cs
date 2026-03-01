using System.Diagnostics.CodeAnalysis;

namespace BouncyHsm.Core.Services.Contracts.Entities;

[Dunet.Union]
public partial record AttributeValueResult
{
    partial record Ok(IAttributeValue Value);
    partial record Computed(Task<IAttributeValue> TaskValue, bool Updated);
    partial record SensitiveOrUnextractable();
    partial record InvalidAttribute();

    public async Task<IAttributeValue?> GetOkOrComputed()
    {
        Task<IAttributeValue>? task = this.Match<Task<IAttributeValue>?>(ok => Task.FromResult(ok.Value),
              computed => computed.TaskValue,
              sensitiveOrUnextractable => null,
              invalidAttribute => null);

        if (task != null)
        {
            return await task;
        }

        return null;
    }

    public sealed override string ToString()
    {
        return this.Match(ok => $"Value {ok.Value}",
            computed => "Computed attribute.",
            sensitiveOrUnextractable => "Sensitive or unextractable attribute.",
            invalidAttribute => "Attribute is invalid for object.");
    }
}