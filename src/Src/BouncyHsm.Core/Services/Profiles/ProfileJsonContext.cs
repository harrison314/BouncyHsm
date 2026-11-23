using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using System.Text.Json.Serialization;

namespace BouncyHsm.Core.Services.Profiles;

[JsonSourceGenerationOptions(WriteIndented = true,
    Converters = [typeof(JsonStringEnumConverter<CKM>), typeof(JsonStringEnumConverter<MechanismCkf>), typeof(JsonStringEnumConverter<Pkcs11SpecVersion>)],
    ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    )]
[JsonSerializable(typeof(Profile))]
internal partial class ProfileJsonContext : JsonSerializerContext
{
}
