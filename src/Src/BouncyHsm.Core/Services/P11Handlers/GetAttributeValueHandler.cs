using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;
using System.Text;
using static BouncyHsm.Core.Services.Contracts.Entities.AttributeValueResult;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class GetAttributeValueHandler : IRpcRequestHandler<GetAttributeValueRequest, GetAttributeValueEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<GetAttributeValueHandler> logger;

    public GetAttributeValueHandler(IP11HwServices hwServices, ILogger<GetAttributeValueHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async Task<GetAttributeValueEnvelope> Handle(GetAttributeValueRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId} object handle {ObjectHandle}.",
            request.SessionId,
            request.ObjectHandle);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        ICryptoApiObject pkcs11Object = await this.hwServices.FindObjectByHandle(memorySession,
             p11Session,
             request.ObjectHandle,
             cancellationToken);

        this.logger.LogDebug("Load object {object}.", pkcs11Object.ToString());

        GetAttributeOutValue[] values = new GetAttributeOutValue[request.InTemplate.Length];
        CKR rv = CKR.CKR_OK;

        for (int i = 0; i < values.Length; i++)
        {
            GetAttributeInputValues td = request.InTemplate[i];
            CKA attributeType = (CKA)td.AttributeType;
            this.logger.LogTrace("Process attribute {attributeType} with IsValuePtrSet {IsValuePtrSet} on position {position}.",
                attributeType,
                td.IsValuePtrSet,
                i);

            AttributeValueResult attributeValueResult = pkcs11Object.GetValue(attributeType);
            GetAttributeOutValue outValue = new GetAttributeOutValue();

            outValue.ValueLen = this.GuessValueLength(attributeValueResult);
            this.UpdateCkr(ref rv, attributeValueResult);

            if (attributeValueResult.IsOK(out IAttributeValue? attributeValue))
            {
                this.SetOutValue(ref outValue, attributeValue);
                this.logger.LogDebug("Return attribute {attributeType} with value type {valueType} on position {position}.",
                    attributeType,
                    attributeValue.TypeTag,
                    i);
            }
            else
            {
                outValue.ValueType = NativeAttributeValue.AttrValueToNativeTypeVoid;
                if (Enum.IsDefined<CKA>(attributeType))
                {
                    this.logger.LogWarning("Return attribute {attributeType} with error {errorType} on position {position}.",
                        attributeType,
                        attributeValueResult,
                        i);
                }
                else
                {
                    this.logger.LogWarning("Return attribute {attributeType} ({attributeTypeHex}) with error {errorType} on position {position}.",
                        attributeType,
                        string.Concat("0x", ((uint)attributeType).ToString("X8")),
                        attributeValueResult,
                        i);
                }
            }

            values[i] = outValue;
        }

        // Posable attribute too small on client
        return new GetAttributeValueEnvelope()
        {
            Rv = (uint)rv,
            Data = new GetAttributeOutValues()
            {
                OutTemplate = values
            }
        };
    }

    private CkSpecialUint GuessValueLength(AttributeValueResult attributeValueResult)
    {
        return attributeValueResult.Match<CkSpecialUint>(value => CkSpecialUint.Create(value.Value.GuessSize()),
            sensitiveOrUnextractable => CkSpecialUint.CreateUnavailableInformation(),
            invalidAttribute => CkSpecialUint.CreateUnavailableInformation());
    }

    private void UpdateCkr(ref CKR ckr, AttributeValueResult attributeValueResult)
    {
        if (ckr == CKR.CKR_OK)
        {
            ckr = attributeValueResult.Match<CKR>(ok => CKR.CKR_OK,
                sensitiveOrUnextractable => CKR.CKR_ATTRIBUTE_SENSITIVE,
                invalidAttribute => CKR.CKR_ATTRIBUTE_TYPE_INVALID);
        }
    }

    private void SetOutValue(ref GetAttributeOutValue outValue, IAttributeValue attributeValue)
    {
        switch (attributeValue.TypeTag)
        {
            case AttrTypeTag.ByteArray:
                outValue.ValueType = NativeAttributeValue.AttrValueToNativeTypeByteArray;
                outValue.ValueBytes = attributeValue.AsByteArray();
                break;

            case AttrTypeTag.String:
                outValue.ValueType = NativeAttributeValue.AttrValueToNativeTypeByteArray;
                outValue.ValueBytes = Encoding.UTF8.GetBytes(attributeValue.AsString());
                break;

            case AttrTypeTag.CkBool:
                outValue.ValueType = NativeAttributeValue.AttrValueToNativeTypeBool;
                outValue.ValueBool = attributeValue.AsBool();
                break;

            case AttrTypeTag.CkUint:
                outValue.ValueType = NativeAttributeValue.AttrValueToNativeTypeCkUint;
                outValue.ValueUint = attributeValue.AsUint();
                break;

            case AttrTypeTag.DateTime:
                outValue.ValueType = NativeAttributeValue.AttrValueToNativeTypeCkDate;
                outValue.ValueCkDate = attributeValue.AsDate().ToRpcString();
                break;

            default:
                throw new InvalidProgramException($"Enum value {attributeValue.TypeTag} is not supported.");
        };
    }
}