using MessagePack;
using MessagePack.Resolvers;
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
        return new MessagePackSerializerOptions(CompositeResolver.Create(BuiltinResolver.Instance, MessagepackBouncyHsmResolver.Instance));
    }
}
