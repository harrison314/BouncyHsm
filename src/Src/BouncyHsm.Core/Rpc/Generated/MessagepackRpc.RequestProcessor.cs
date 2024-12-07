//----------------------
// <auto-generated>
//     Generated by the BouncyHsm.RpcGenerator
//     source: RpcDefinition.yaml
// </auto-generated>
//----------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Buffers;

namespace BouncyHsm.Core.Rpc;


#nullable enable

public static partial class RequestProcessor
{

   [System.CodeDom.Compiler.GeneratedCode("BouncyHsm.RpcGenerator.Generators", "1.0.0")]
   private static ValueTask<IMemoryOwner<byte>> ProcessRequestInternal(IServiceProvider serviceProvider, HeaderStructure header, ReadOnlyMemory<byte> requestBody, ILogger logger, CancellationToken cancellationToken)
   {
      return header.Operation switch
      {
        "Ping" => ProcessRequestBody<PingRequest, PingEnvelope>(serviceProvider, "Ping", requestBody, static ckRv => new PingEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "Initialize" => ProcessRequestBody<InitializeRequest, InitializeEnvelope>(serviceProvider, "Initialize", requestBody, static ckRv => new InitializeEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "Finalize" => ProcessRequestBody<FinalizeRequest, FinalizeEnvelope>(serviceProvider, "Finalize", requestBody, static ckRv => new FinalizeEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "GetInfo" => ProcessRequestBody<GetInfoRequest, GetInfoEnvelope>(serviceProvider, "GetInfo", requestBody, static ckRv => new GetInfoEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "GetSlotList" => ProcessRequestBody<GetSlotListRequest, GetSlotListEnvelope>(serviceProvider, "GetSlotList", requestBody, static ckRv => new GetSlotListEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "GetSlotInfo" => ProcessRequestBody<GetSlotInfoRequest, GetSlotInfoEnvelope>(serviceProvider, "GetSlotInfo", requestBody, static ckRv => new GetSlotInfoEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "GetTokenInfo" => ProcessRequestBody<GetTokenInfoRequest, GetTokenInfoEnvelope>(serviceProvider, "GetTokenInfo", requestBody, static ckRv => new GetTokenInfoEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "GetMechanismList" => ProcessRequestBody<GetMechanismListRequest, GetMechanismListEnvelope>(serviceProvider, "GetMechanismList", requestBody, static ckRv => new GetMechanismListEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "GetMechanismInfo" => ProcessRequestBody<GetMechanismInfoRequest, GetMechanismInfoEnvelope>(serviceProvider, "GetMechanismInfo", requestBody, static ckRv => new GetMechanismInfoEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "SetPin" => ProcessRequestBody<SetPinRequest, SetPinEnvelope>(serviceProvider, "SetPin", requestBody, static ckRv => new SetPinEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "OpenSession" => ProcessRequestBody<OpenSessionRequest, OpenSessionEnvelope>(serviceProvider, "OpenSession", requestBody, static ckRv => new OpenSessionEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "CloseSession" => ProcessRequestBody<CloseSessionRequest, CloseSessionEnvelope>(serviceProvider, "CloseSession", requestBody, static ckRv => new CloseSessionEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "CloseAllSessions" => ProcessRequestBody<CloseAllSessionsRequest, CloseAllSessionsEnvelope>(serviceProvider, "CloseAllSessions", requestBody, static ckRv => new CloseAllSessionsEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "GetSessionInfo" => ProcessRequestBody<GetSessionInfoRequest, GetSessionInfoEnvelope>(serviceProvider, "GetSessionInfo", requestBody, static ckRv => new GetSessionInfoEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "Login" => ProcessRequestBody<LoginRequest, LoginEnvelope>(serviceProvider, "Login", requestBody, static ckRv => new LoginEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "Logout" => ProcessRequestBody<LogoutRequest, LogoutEnvelope>(serviceProvider, "Logout", requestBody, static ckRv => new LogoutEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "SeedRandom" => ProcessRequestBody<SeedRandomRequest, SeedRandomEnvelope>(serviceProvider, "SeedRandom", requestBody, static ckRv => new SeedRandomEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "GenerateRandom" => ProcessRequestBody<GenerateRandomRequest, GenerateRandomEnvelope>(serviceProvider, "GenerateRandom", requestBody, static ckRv => new GenerateRandomEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "DigestInit" => ProcessRequestBody<DigestInitRequest, DigestInitEnvelope>(serviceProvider, "DigestInit", requestBody, static ckRv => new DigestInitEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "Digest" => ProcessRequestBody<DigestRequest, DigestEnvelope>(serviceProvider, "Digest", requestBody, static ckRv => new DigestEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "DigestUpdate" => ProcessRequestBody<DigestUpdateRequest, DigestUpdateEnvelope>(serviceProvider, "DigestUpdate", requestBody, static ckRv => new DigestUpdateEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "DigestKey" => ProcessRequestBody<DigestKeyRequest, DigestKeyEnvelope>(serviceProvider, "DigestKey", requestBody, static ckRv => new DigestKeyEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "DigestFinal" => ProcessRequestBody<DigestFinalRequest, DigestFinalEnvelope>(serviceProvider, "DigestFinal", requestBody, static ckRv => new DigestFinalEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "CreateObject" => ProcessRequestBody<CreateObjectRequest, CreateObjectEnvelope>(serviceProvider, "CreateObject", requestBody, static ckRv => new CreateObjectEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "DestroyObject" => ProcessRequestBody<DestroyObjectRequest, DestroyObjectEnvelope>(serviceProvider, "DestroyObject", requestBody, static ckRv => new DestroyObjectEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "FindObjectsInit" => ProcessRequestBody<FindObjectsInitRequest, FindObjectsInitEnvelope>(serviceProvider, "FindObjectsInit", requestBody, static ckRv => new FindObjectsInitEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "FindObjects" => ProcessRequestBody<FindObjectsRequest, FindObjectsEnvelope>(serviceProvider, "FindObjects", requestBody, static ckRv => new FindObjectsEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "FindObjectsFinal" => ProcessRequestBody<FindObjectsFinalRequest, FindObjectsFinalEnvelope>(serviceProvider, "FindObjectsFinal", requestBody, static ckRv => new FindObjectsFinalEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "GetObjectSize" => ProcessRequestBody<GetObjectSizeRequest, GetObjectSizeEnvelope>(serviceProvider, "GetObjectSize", requestBody, static ckRv => new GetObjectSizeEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "GetAttributeValue" => ProcessRequestBody<GetAttributeValueRequest, GetAttributeValueEnvelope>(serviceProvider, "GetAttributeValue", requestBody, static ckRv => new GetAttributeValueEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "GenerateKeyPair" => ProcessRequestBody<GenerateKeyPairRequest, GenerateKeyPairEnvelope>(serviceProvider, "GenerateKeyPair", requestBody, static ckRv => new GenerateKeyPairEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "SetAttributeValue" => ProcessRequestBody<SetAttributeValueRequest, SetAttributeValueEnvelope>(serviceProvider, "SetAttributeValue", requestBody, static ckRv => new SetAttributeValueEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "CopyObject" => ProcessRequestBody<CopyObjectRequest, CopyObjectEnvelope>(serviceProvider, "CopyObject", requestBody, static ckRv => new CopyObjectEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "SignInit" => ProcessRequestBody<SignInitRequest, SignInitEnvelope>(serviceProvider, "SignInit", requestBody, static ckRv => new SignInitEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "Sign" => ProcessRequestBody<SignRequest, SignEnvelope>(serviceProvider, "Sign", requestBody, static ckRv => new SignEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "SignUpdate" => ProcessRequestBody<SignUpdateRequest, SignUpdateEnvelope>(serviceProvider, "SignUpdate", requestBody, static ckRv => new SignUpdateEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "SignFinal" => ProcessRequestBody<SignFinalRequest, SignFinalEnvelope>(serviceProvider, "SignFinal", requestBody, static ckRv => new SignFinalEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "VerifyInit" => ProcessRequestBody<VerifyInitRequest, VerifyInitEnvelope>(serviceProvider, "VerifyInit", requestBody, static ckRv => new VerifyInitEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "Verify" => ProcessRequestBody<VerifyRequest, VerifyEnvelope>(serviceProvider, "Verify", requestBody, static ckRv => new VerifyEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "VerifyUpdate" => ProcessRequestBody<VerifyUpdateRequest, VerifyUpdateEnvelope>(serviceProvider, "VerifyUpdate", requestBody, static ckRv => new VerifyUpdateEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "VerifyFinal" => ProcessRequestBody<VerifyFinalRequest, VerifyFinalEnvelope>(serviceProvider, "VerifyFinal", requestBody, static ckRv => new VerifyFinalEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "GenerateKey" => ProcessRequestBody<GenerateKeyRequest, GenerateKeyEnvelope>(serviceProvider, "GenerateKey", requestBody, static ckRv => new GenerateKeyEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "DeriveKey" => ProcessRequestBody<DeriveKeyRequest, DeriveKeyEnvelope>(serviceProvider, "DeriveKey", requestBody, static ckRv => new DeriveKeyEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "EncryptInit" => ProcessRequestBody<EncryptInitRequest, EncryptInitEnvelope>(serviceProvider, "EncryptInit", requestBody, static ckRv => new EncryptInitEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "Encrypt" => ProcessRequestBody<EncryptRequest, EncryptEnvelope>(serviceProvider, "Encrypt", requestBody, static ckRv => new EncryptEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "EncryptUpdate" => ProcessRequestBody<EncryptUpdateRequest, EncryptUpdateEnvelope>(serviceProvider, "EncryptUpdate", requestBody, static ckRv => new EncryptUpdateEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "EncryptFinal" => ProcessRequestBody<EncryptFinalRequest, EncryptFinalEnvelope>(serviceProvider, "EncryptFinal", requestBody, static ckRv => new EncryptFinalEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "DecryptInit" => ProcessRequestBody<DecryptInitRequest, DecryptInitEnvelope>(serviceProvider, "DecryptInit", requestBody, static ckRv => new DecryptInitEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "Decrypt" => ProcessRequestBody<DecryptRequest, DecryptEnvelope>(serviceProvider, "Decrypt", requestBody, static ckRv => new DecryptEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "DecryptUpdate" => ProcessRequestBody<DecryptUpdateRequest, DecryptUpdateEnvelope>(serviceProvider, "DecryptUpdate", requestBody, static ckRv => new DecryptUpdateEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "DecryptFinal" => ProcessRequestBody<DecryptFinalRequest, DecryptFinalEnvelope>(serviceProvider, "DecryptFinal", requestBody, static ckRv => new DecryptFinalEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "WrapKey" => ProcessRequestBody<WrapKeyRequest, WrapKeyEnvelope>(serviceProvider, "WrapKey", requestBody, static ckRv => new WrapKeyEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "UnwrapKey" => ProcessRequestBody<UnwrapKeyRequest, UnwrapKeyEnvelope>(serviceProvider, "UnwrapKey", requestBody, static ckRv => new UnwrapKeyEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "WaitForSlotEvent" => ProcessRequestBody<WaitForSlotEventRequest, WaitForSlotEventEnvelope>(serviceProvider, "WaitForSlotEvent", requestBody, static ckRv => new WaitForSlotEventEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "SignRecoverInit" => ProcessRequestBody<SignRecoverInitRequest, SignRecoverInitEnvelope>(serviceProvider, "SignRecoverInit", requestBody, static ckRv => new SignRecoverInitEnvelope(){ Rv = ckRv }, logger, cancellationToken),
        "SignRecover" => ProcessRequestBody<SignRecoverRequest, SignRecoverEnvelope>(serviceProvider, "SignRecover", requestBody, static ckRv => new SignRecoverEnvelope(){ Rv = ckRv }, logger, cancellationToken),
          _ => throw new InvalidOperationException($"RPC operation {header.Operation} is not supported.")
      };
   }
}
