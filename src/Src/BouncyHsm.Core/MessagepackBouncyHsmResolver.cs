using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core;

[GeneratedMessagePackResolver]
internal partial class MessagepackBouncyHsmResolver
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static MessagePackSerializerOptions? GetOptions()
    {
        // TODO: Fix problems with MessagePack - System.String is not registered in resolver: BouncyHsm.Core.MessagepackBouncyHsmResolver'
        // return new MessagePackSerializerOptions(MessagepackBouncyHsmResolver.Instance);
        return null;
    }
}
