
using BouncyHsm.Core.UseCases.Contracts;

namespace BouncyHsm.Models.HsmInfo;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.FunctionImplState))]
public record FunctionImplStateDto(string FunctionName, ImplementationState State);