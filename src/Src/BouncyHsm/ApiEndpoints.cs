using BouncyHsm.Core.Infrastructure.Extensions;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Controllers;
using BouncyHsm.Models.ApplicationConnection;
using BouncyHsm.Models.HsmInfo;
using BouncyHsm.Models.KeyGeneration;
using BouncyHsm.Models.Migration;
using BouncyHsm.Models.Pkcs;
using BouncyHsm.Models.Slot;
using BouncyHsm.Models.Stats;
using BouncyHsm.Models.StorageObjects;
using System.ComponentModel.DataAnnotations;

namespace BouncyHsm;

internal static class ApiEndpointRouteExtensions
{
    public static IEndpointRouteBuilder MapBouncyHsmApi(this IEndpointRouteBuilder endpoints)
    {
        ApiEndpoints.MapApplicationConnectionEndpoints(endpoints.MapGroup("/ApplicationConnection"));
        ApiEndpoints.MapHsmInfoEndpoints(endpoints.MapGroup("/HsmInfo"));
        ApiEndpoints.MapKeyGenerationEndpoints(endpoints.MapGroup("/KeyGeneration"));
        ApiEndpoints.MapMigrationEndpoints(endpoints.MapGroup("/Migration"));
        ApiEndpoints.MapPkcsEndpoints(endpoints.MapGroup("/Pkcs"));
        ApiEndpoints.MapSlotEndpoints(endpoints.MapGroup("/Slot"));
        ApiEndpoints.MapStatsEndpoints(endpoints.MapGroup("/Stats"));
        ApiEndpoints.MapStorageObjectsEndpoints(endpoints.MapGroup("/StorageObjects"));
        return endpoints;
    }
}

internal sealed class ApiEndpoints
{

    private static RouteGroupBuilder ApiGroup(RouteGroupBuilder group)
    {
        return group.ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    internal static void MapApplicationConnectionEndpoints(RouteGroupBuilder group)
    {
        group = ApiGroup(group);
        group.MapGet("/", async (IApplicationConnectionsFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GetApplicationConnections");
            DomainResult<SlotConnections> result = await facade.GetApplicationConnections(context.RequestAborted);
            return result.MapOk(t => t.ApplicationSessions)
                .MapOk(ApplicationConnectionControllerMapper.ToDto)
                .ToMinimalApiResult();
        }).WithName("GetApplicationConnections").Produces<List<ApplicationSessionDto>>(200);

        group.MapDelete("/{applicationSessionId}", async (Guid applicationSessionId, IApplicationConnectionsFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to RemoveApplicationConnection with {applicationSessionId}.", applicationSessionId);
            return (await facade.RemoveApplicationConnection(applicationSessionId, context.RequestAborted)).ToMinimalApiResult();
        }).WithName("RemoveApplicationConnection").Produces(200);
    }

    internal static void MapHsmInfoEndpoints(RouteGroupBuilder group)
    {
        group = ApiGroup(group);
        group.MapGet("/Versions", (IHsmInfoFacade facade, ILogger<ApiEndpoints> logger) =>
        {
            logger.LogTrace("Entering to GetVersions");
            return Results.Ok(HsmInfoControllerMapper.ToDto(facade.GetVersions()));
        }).WithName("GetVersions").Produces<BouncyHsmVersionDto>(200);

        group.MapGet("/SupportedKeys", (IHsmInfoFacade facade, ILogger<ApiEndpoints> logger) =>
        {
            logger.LogTrace("Entering to GetSupportedKeys");
            return Results.Ok(HsmInfoControllerMapper.ToDto(facade.GetSupportedKeys()));
        }).WithName("GetSupportedKeys").Produces<SupportedKeysDto>(200);

        group.MapGet("/GetMechanism", (IHsmInfoFacade facade, ILogger<ApiEndpoints> logger) =>
        {
            logger.LogTrace("Entering to GetMechanism");
            return Results.Ok(HsmInfoControllerMapper.ToDto(facade.GetAllMechanism()));
        }).WithName("GetMechanism").Produces<MechanismProfileDto>(200);

        group.MapGet("/GetFunctionsState", (IHsmInfoFacade facade, ILogger<ApiEndpoints> logger) =>
        {
            logger.LogTrace("Entering to GetFunctionsState");
            return Results.Ok(HsmInfoControllerMapper.ToDto(facade.GetFunctionsState()));
        }).WithName("GetFunctionsState").Produces<IEnumerable<FunctionImplStateDto>>(200);
    }

    internal static void MapKeyGenerationEndpoints(RouteGroupBuilder group)
    {
        group = ApiGroup(group);
        group.MapPost("/{slotId}/GenerateRsaKeyPair", async (uint slotId, GenerateRsaKeyPairRequestDto model, IKeysGenerationFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GenerateRsaKeyPair with slotId {slotId}.", slotId);
            GenerateRsaKeyPairRequest request = KeyGenerationControllerMapper.MapFromDto(model);
            DomainResult<GeneratedKeyPairIds> result = await facade.GenerateRsaKeyPair(slotId, request, context.RequestAborted);
            return result.MapOk(KeyGenerationControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GenerateRsaKeyPair").Produces<GeneratedKeyPairIdsDto>(200);

        group.MapPost("/{slotId}/GenerateEcKeyPair", async (uint slotId, GenerateEcKeyPairRequestDto model, IKeysGenerationFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GenerateEcKeyPair with slotId {slotId}.", slotId);
            GenerateEcKeyPairRequest request = KeyGenerationControllerMapper.MapFromDto(model);
            DomainResult<GeneratedKeyPairIds> result = await facade.GenerateEcKeyPair(slotId, request, context.RequestAborted);
            return result.MapOk(KeyGenerationControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GenerateEcKeyPair").Produces<GeneratedKeyPairIdsDto>(200);

        group.MapPost("/{slotId}/GenerateEdwardsKeyPair", async (uint slotId, GenerateEdwardsKeyPairRequestDto model, IKeysGenerationFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GenerateEdwardsKeyPair with slotId {slotId}.", slotId);
            GenerateEdwardsKeyPairRequest request = KeyGenerationControllerMapper.MapFromDto(model);
            DomainResult<GeneratedKeyPairIds> result = await facade.GenerateEdwardsKeyPair(slotId, request, context.RequestAborted);
            return result.MapOk(KeyGenerationControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GenerateEdwardsKeyPair").Produces<GeneratedKeyPairIdsDto>(200);

        group.MapPost("/{slotId}/GenerateMontgomeryKeyPair", async (uint slotId, GenerateMontgomeryKeyPairRequestDto model, IKeysGenerationFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GenerateMontgomeryKeyPair with slotId {slotId}.", slotId);
            GenerateMontgomeryKeyPairRequest request = KeyGenerationControllerMapper.MapFromDto(model);
            DomainResult<GeneratedKeyPairIds> result = await facade.GenerateMontgomeryKeyPair(slotId, request, context.RequestAborted);
            return result.MapOk(KeyGenerationControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GenerateMontgomeryKeyPair").Produces<GeneratedKeyPairIdsDto>(200);

        group.MapPost("/{slotId}/GenerateMlDsaKeyPair", async (uint slotId, GenerateMLDsaKeyPairRequestDto model, IKeysGenerationFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GenerateMlDsaKeyPair with slotId {slotId}.", slotId);
            GenerateMLDsaKeyPairRequest request = KeyGenerationControllerMapper.MapFromDto(model);
            DomainResult<GeneratedKeyPairIds> result = await facade.GenerateMLDsaKeyPair(slotId, request, context.RequestAborted);
            return result.MapOk(KeyGenerationControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GenerateMlDsaKeyPair").Produces<GeneratedKeyPairIdsDto>(200);

        group.MapPost("/{slotId}/GenerateSlhDsaKeyPair", async (uint slotId, GenerateSlhDsaKeyPairRequestDto model, IKeysGenerationFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GenerateSlhDsaKeyPair with slotId {slotId}.", slotId);
            GenerateSlhDsaKeyPairRequest request = KeyGenerationControllerMapper.MapFromDto(model);
            DomainResult<GeneratedKeyPairIds> result = await facade.GenerateSlhDsaKeyPair(slotId, request, context.RequestAborted);
            return result.MapOk(KeyGenerationControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GenerateSlhDsaKeyPair").Produces<GeneratedKeyPairIdsDto>(200);

        group.MapPost("/{slotId}/GenerateHssKeyPair", async (uint slotId, GenerateHssKeyPairRequestDto model, IKeysGenerationFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GenerateHssKeyPair with slotId {slotId}.", slotId);
            GenerateHssKeyPairRequest request;
            try
            {
                request = KeyGenerationControllerMapper.MapFromDto(model);
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid HSS request.");
                return new DomainResult<GeneratedKeyPairIds>.InvalidInput(ex.Message).ToMinimalApiResult();
            }

            DomainResult<GeneratedKeyPairIds> result = await facade.GenerateHssKeyPair(slotId, request, context.RequestAborted);
            return result.MapOk(KeyGenerationControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GenerateHssKeyPair").Produces<GeneratedKeyPairIdsDto>(200);

        group.MapPost("/{slotId}/GenerateMlKemKeyPair", async (uint slotId, GenerateMlKemKeyPairRequestDto model, IKeysGenerationFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GenerateMlKemKeyPair with slotId {slotId}.", slotId);
            GenerateMLKemKeyPairRequest request = KeyGenerationControllerMapper.MapFromDto(model);
            DomainResult<GeneratedKeyPairIds> result = await facade.GenerateMLKemKeyPair(slotId, request, context.RequestAborted);
            return result.MapOk(KeyGenerationControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GenerateMlKemKeyPair").Produces<GeneratedKeyPairIdsDto>(200);

        group.MapPost("/{slotId}/GenerateAesKey", async (uint slotId, GenerateAesKeyRequestDto model, IKeysGenerationFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GenerateAesKey with slotId {slotId}.", slotId);
            GenerateAesKeyRequest request = KeyGenerationControllerMapper.MapFromDto(model);
            DomainResult<GeneratedSecretId> result = await facade.GenerateAesKey(slotId, request, context.RequestAborted);
            return result.MapOk(KeyGenerationControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GenerateAesKey").Produces<GeneratedSecretIdDto>(200);

        group.MapPost("/{slotId}/GeneratePoly1305Key", async (uint slotId, GeneratePoly1305KeyRequestDto model, IKeysGenerationFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GeneratePoly1305Key with slotId {slotId}.", slotId);
            GeneratePoly1305KeyRequest request = KeyGenerationControllerMapper.MapFromDto(model);
            DomainResult<GeneratedSecretId> result = await facade.GeneratePoly1305Key(slotId, request, context.RequestAborted);
            return result.MapOk(KeyGenerationControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GeneratePoly1305Key").Produces<GeneratedSecretIdDto>(200);

        group.MapPost("/{slotId}/GenerateChaCha20Key", async (uint slotId, GenerateChaCha20KeyRequestDto model, IKeysGenerationFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GenerateChaCha20Key with slotId {slotId}.", slotId);
            GenerateChaCha20KeyRequest request = KeyGenerationControllerMapper.MapFromDto(model);
            DomainResult<GeneratedSecretId> result = await facade.GenerateChaCha20Key(slotId, request, context.RequestAborted);
            return result.MapOk(KeyGenerationControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GenerateChaCha20Key").Produces<GeneratedSecretIdDto>(200);

        group.MapPost("/{slotId}/GenerateSalsa20Key", async (uint slotId, GenerateSalsa20KeyRequestDto model, IKeysGenerationFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GenerateSalsa20Key with slotId {slotId}.", slotId);
            GenerateSalsa20KeyRequest request = KeyGenerationControllerMapper.MapFromDto(model);
            DomainResult<GeneratedSecretId> result = await facade.GenerateSalsa20Key(slotId, request, context.RequestAborted);
            return result.MapOk(KeyGenerationControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GenerateSalsa20Key").Produces<GeneratedSecretIdDto>(200);

        group.MapPost("/{slotId}/GenerateSecretKey", async (uint slotId, GenerateSecretKeyRequestDto model, IKeysGenerationFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GenerateSecretKey with slotId {slotId}.", slotId);
            GenerateSecretKeyRequest request = KeyGenerationControllerMapper.MapFromDto(model);
            DomainResult<GeneratedSecretId> result = await facade.GenerateSecretKey(slotId, request, context.RequestAborted);
            return result.MapOk(KeyGenerationControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GenerateSecretKey").Produces<GeneratedSecretIdDto>(200);

        group.MapPost("/{slotId}/GenerateCamelliaKey", async (uint slotId, GenerateCamelliaKeyRequestDto model, IKeysGenerationFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GenerateCamelliaKey with slotId {slotId}.", slotId);
            GenerateCamelliaKeyRequest request = KeyGenerationControllerMapper.MapFromDto(model);
            DomainResult<GeneratedSecretId> result = await facade.GenerateCamelliaKey(slotId, request, context.RequestAborted);
            return result.MapOk(KeyGenerationControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GenerateCamelliaKey").Produces<GeneratedSecretIdDto>(200);
    }

    internal static void MapMigrationEndpoints(RouteGroupBuilder group)
    {
        group = ApiGroup(group);
        group.MapPost("/", async (MigrationRequestDto model, IMigrationFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to Migrate");
            MigrationRequest request = MigrationControllerMapper.MapFromDto(model);
            DomainResult<MigrationResult> result = await facade.Migrate(request, context.RequestAborted);
            return result.MapOk(MigrationControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("Migrate").Produces<MigrationResultDto>(200);
    }

    internal static void MapPkcsEndpoints(RouteGroupBuilder group)
    {
        group = ApiGroup(group);
        group.MapPost("/{slotId}/GeneratePkcs10", async (uint slotId, GeneratePkcs10RequestDto model, IPkcsFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GeneratePkcs10 with slotId {slotId}.", slotId);
            GeneratePkcs10Request request = PkcsControllerMapper.FromDto(model, slotId);
            DomainResult<byte[]> result = await facade.GeneratePkcs10(request, context.RequestAborted);
            return result.MapOk(t => new Pkcs10Dto(t)).ToMinimalApiResult();
        }).WithName("GeneratePkcs10").Produces<Pkcs10Dto>(200);

        group.MapPost("/{slotId}/GenerateSelfSignedCert", async (uint slotId, GenerateSelfSignedCertRequestDto model, IPkcsFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GenerateSelfSignedCert with slotId {slotId}.", slotId);
            GenerateSelfSignedCertRequest request = PkcsControllerMapper.FromDto(model, slotId);
            DomainResult<Guid> result = await facade.GenerateSelfSignedCert(request, context.RequestAborted);
            return result.MapOk(t => new GenerateSelfSignedCertResponseDto(t)).ToMinimalApiResult();
        }).WithName("GenerateSelfSignedCert").Produces<GenerateSelfSignedCertResponseDto>(200);

        group.MapGet("/{slotId}", async (uint slotId, IPkcsFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GetPkcsObjects with slotId {slotId}.", slotId);
            DomainResult<PkcsObjects> result = await facade.GetObjects(slotId, context.RequestAborted);
            return result.MapOk(PkcsControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GetPkcsObjects").Produces<PkcsObjectsDto>(200);

        group.MapPost("/{slotId}/ImportP12", async (uint slotId, ImportP12RequestDto model, IPkcsFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to ImportP12 with slotId {slotId}.", slotId);
            ImportP12Request request = PkcsControllerMapper.FromDto(model, slotId);
            DomainResult<Guid> privateKeyId = await facade.ImportP12(request, context.RequestAborted);
            return privateKeyId.MapOk(t => new ImportP12ResponseDto() { PrivateKeyId = t }).ToMinimalApiResult();
        }).WithName("ImportP12").Produces<ImportP12ResponseDto>(200);

        group.MapPost("/{slotId}/ImportPem", async (uint slotId, ImportPemRequestDto model, IPkcsFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to ImportPem with slotId {slotId}.", slotId);
            ImportPemRequest request = PkcsControllerMapper.FromDto(model, slotId);
            DomainResult<IReadOnlyList<Guid>> objectIds = await facade.ImportPem(request, context.RequestAborted);
            return objectIds.MapOk(t => new ImportPemResponseDto(t)).ToMinimalApiResult();
        }).WithName("ImportPem").Produces<ImportPemResponseDto>(200);

        group.MapPost("/{slotId}/ImportX509Certificate", async (uint slotId, ImportX509CertificateRequestDto model, IPkcsFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to ImportX509Certificate with slotId {slotId}.", slotId);
            ImportX509CertificateRequest request = PkcsControllerMapper.FromDto(model, slotId);
            DomainResult<Guid> result = await facade.ImportX509Certificate(request, context.RequestAborted);
            return result.MapOkToVoid().ToMinimalApiResult();
        }).WithName("ImportX509Certificate").Produces(200);

        group.MapDelete("/{slotId}/AssociatedObjects/{objectId}", async (uint slotId, Guid objectId, IPkcsFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to DeleteAssociatedObject with slotId {slotId}, objectId {objectId}.", slotId, objectId);
            VoidDomainResult result = await facade.DeleteAssociatedObjects(slotId, objectId, context.RequestAborted);
            return result.ToMinimalApiResult();
        }).WithName("DeleteAssociatedObject").Produces(200);

        group.MapGet("/{slotId}/ParseCertificate/{objectId}", async (uint slotId, Guid objectId, IPkcsFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to ParseCertificate with slotId {slotId}, objectId {objectId}.", slotId, objectId);
            DomainResult<CertificateDetail> result = await facade.ParseCertificate(slotId, objectId, context.RequestAborted);
            return result.MapOk(PkcsControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("ParseCertificate").Produces<CertificateDetailDto>(200);
    }

    internal static void MapSlotEndpoints(RouteGroupBuilder group)
    {
        group = ApiGroup(group);
        group.MapPost("/", async (CreateSlotDto model, ISlotFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to CreateSlot");
            CreateSlotData createRequest = SlotControllerMapper.MapFromDto(model);
            DomainResult<CreateSlotResult> result = await facade.CreateSlot(createRequest, context.RequestAborted);
            return result.MapOk(SlotControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("CreateSlot").Produces<CreateSlotResultDto>(200);

        group.MapGet("/", async (ISlotFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GetAllSlots");
            DomainResult<IReadOnlyList<SlotEntity>> result = await facade.GetAllSlots(context.RequestAborted);
            return result.MapOk(SlotControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GetAllSlots").Produces<List<SlotDto>>(200);

        group.MapGet("/{slotId}", async (uint slotId, ISlotFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GetSlot with slotId {slotId}.", slotId);
            DomainResult<SlotEntity> result = await facade.GetSlotById(slotId, context.RequestAborted);
            return result.MapOk(SlotControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GetSlot").Produces<SlotDto>(200);

        group.MapDelete("/{slotId}", async (uint slotId, ISlotFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to DeleteSlot with slotId {slotId}.", slotId);
            VoidDomainResult result = await facade.DeleteSlot(slotId, context.RequestAborted);
            return result.ToMinimalApiResult();
        }).WithName("DeleteSlot").Produces(200);

        group.MapPost("/{slotId}/PluggedState", async (uint slotId, SetPluggedStateDto model, ISlotFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to SetSlotPluggedState with {slotId}, plugged {plugged}", slotId, model.Plugged);
            VoidDomainResult result = await facade.SetPluggedState(slotId, model.Plugged, context.RequestAborted);
            return result.ToMinimalApiResult();
        }).WithName("SetSlotPluggedState").Produces(200);

        group.MapPost("/{slotId}/TokenPin", async (uint slotId, SetTokenPinDataDto model, ISlotFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to SetTokenPinForSlot with {slotId}, userType {userType}", slotId, model.UserType);
            SetTokenPinData data = SlotControllerMapper.MapFromDto(model);
            VoidDomainResult result = await facade.SetTokenPin(slotId, data, context.RequestAborted);
            return result.ToMinimalApiResult();
        }).WithName("SetTokenPinForSlot").Produces(200);
    }

    internal static void MapStatsEndpoints(RouteGroupBuilder group)
    {
        group = ApiGroup(group);
        group.MapGet("/", async (IStatsFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GetOverviewStats");
            DomainResult<OverviewStats> result = await facade.GetOverviewStats(context.RequestAborted);
            return result.MapOk(StatsControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GetOverviewStats").Produces<OverviewStatsDto>(200);
    }

    internal static void MapStorageObjectsEndpoints(RouteGroupBuilder group)
    {
        group = ApiGroup(group);
        group.MapGet("/{slotId}", async (uint slotId, [Range(0, int.MaxValue)] int? skip, [Range(0, int.MaxValue)] int? take, IStorageObjectsFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GetStorageObjects");
            DomainResult<StorageObjectsList> result = await facade.GetStorageObjects(slotId, skip ?? 0, take ?? int.MaxValue, context.RequestAborted);
            return result.MapOk(StorageObjectsControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GetStorageObjects").Produces<StorageObjectsListDto>(200);

        group.MapGet("/{slotId}/{objectId}", async (uint slotId, Guid objectId, IStorageObjectsFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GetStorageObject with slotId {slotId}, objectId {objectId}.", slotId, objectId);
            DomainResult<StorageObjectDetail> result = await facade.GetStorageObject(slotId, objectId, context.RequestAborted);
            return result.MapOk(StorageObjectsControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GetStorageObject").Produces<StorageObjectDetailDto>(200);

        group.MapDelete("/{slotId}/{objectId}", async (uint slotId, Guid objectId, IStorageObjectsFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to RemoveStorageObject with slotId {slotId}, objectId {objectId}.", slotId, objectId);
            VoidDomainResult result = await facade.DeleteStorageObject(slotId, objectId, context.RequestAborted);
            return result.ToMinimalApiResult();
        }).WithName("RemoveStorageObject").Produces(200);

        group.MapGet("/{slotId}/{objectId}/Content", async (uint slotId, Guid objectId, IStorageObjectsFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GetObjectContent with slotId {slotId}, objectId {objectId}.", slotId, objectId);
            DomainResult<ObjectContent> result = await facade.Download(slotId, objectId, context.RequestAborted);
            return result.MapOk(StorageObjectsControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GetObjectContent").Produces<ObjectContentDto>(200);

        group.MapGet("/{slotId}/{objectId}/Attribute/{attributeName}", async (uint slotId, Guid objectId, string attributeName, IStorageObjectsFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to GetAttribute with slotId {slotId}, objectId {objectId}, attributeName {attributeName}.", slotId, objectId, attributeName);
            DomainResult<HighLevelAttributeValue> result = await facade.GetObjectAttribute(slotId, objectId, attributeName, context.RequestAborted);
            return result.MapOk(StorageObjectsControllerMapper.ToDto).ToMinimalApiResult();
        }).WithName("GetAttribute").Produces<HighLevelAttributeValueDto>(200);

        group.MapPost("/{slotId}/{objectId}/Attribute/{attributeName}", async (uint slotId, Guid objectId, string attributeName, HighLevelAttributeValueDto model, IStorageObjectsFacade facade, ILogger<ApiEndpoints> logger, HttpContext context) =>
        {
            logger.LogTrace("Entering to SetAttribute with slotId {slotId}, objectId {objectId}, attributeName {attributeName}.", slotId, objectId, attributeName);
            HighLevelAttributeValue attributeValue = StorageObjectsControllerMapper.FromDto(model);
            VoidDomainResult result = await facade.SetObjectAttribute(slotId, objectId, attributeName, attributeValue, context.RequestAborted);
            return result.ToMinimalApiResult();
        }).WithName("SetAttribute").Produces(200);
    }
}
