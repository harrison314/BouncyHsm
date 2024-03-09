using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Profiles;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Operation")]
[JsonDerivedType(typeof(EnableProfileOperation), typeDiscriminator: "Enable")]
[JsonDerivedType(typeof(RemoveAllProfileOperation), typeDiscriminator: "RemoveAll")]
[JsonDerivedType(typeof(RemoveProfileOperation), typeDiscriminator: "Remove")]
[JsonDerivedType(typeof(AddProfileOperation), typeDiscriminator: "Add")]
[JsonDerivedType(typeof(UpdateProfileOperation), typeDiscriminator: "Update")]
[JsonDerivedType(typeof(RemoveUsingRegexProfileOperation), typeDiscriminator: "RemoveUsingRegex")]
[JsonDerivedType(typeof(EnableUsingRegexProfileOperation), typeDiscriminator: "EnableUsingRegex")]
public abstract class ProfileOperation
{
    public abstract void Update(ref Dictionary<CKM, MechanismInfo> mechanisms, Dictionary<CKM, MechanismInfo> originalMechanisms);
}

