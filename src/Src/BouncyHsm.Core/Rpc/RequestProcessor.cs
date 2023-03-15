using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using MessagePack;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Rpc;

public static partial class RequestProcessor
{
    public class RequestProcessorLogger
    {

    }

    public static async ValueTask<ResponseValue> Process(IServiceProvider scopeProvider, ReadOnlyMemory<byte> requestHeader, ReadOnlyMemory<byte> requestBody, CancellationToken cancellationToken)
    {

        ILogger<RequestProcessorLogger>? logger = (ILogger<RequestProcessorLogger>?)scopeProvider.GetService(typeof(ILogger<RequestProcessorLogger>));
        System.Diagnostics.Debug.Assert(logger != null);

        //#if DEBUG
        //        logger.LogDebug("Request low-level info:\nHead: {0}\nBody:{1}",
        //            BitConverter.ToString(requestHeader.ToArray()),
        //            BitConverter.ToString(requestBody.ToArray()));
        //#endif

        HeaderStructure header = MessagePackSerializer.Deserialize<HeaderStructure>(requestHeader);

        using IDisposable logScope = logger.BeginScope(CreateContextScope(header));

        IMemoryOwner<byte> responseBody = await ProcessRequestInternal(scopeProvider, header, requestBody, logger, cancellationToken);

        OwnedBufferWriter headerWriter = new OwnedBufferWriter(256);
        MessagePackSerializer.Serialize<ResponseHeaderStructure>(headerWriter, new ResponseHeaderStructure());

        //#if DEBUG
        //        logger.LogDebug("Response low-level info:\nHead: {0}\nBody:{1}",
        //            BitConverter.ToString(responseHeader.ToArray()),
        //            BitConverter.ToString(responseBody.ToArray()));
        //#endif

        return new ResponseValue(headerWriter, responseBody);
    }

    private static Dictionary<string, object> CreateContextScope(HeaderStructure header)
    {
        Dictionary<string, object> context = new Dictionary<string, object>()
        {
            { "Operation", header.Operation }
        };

        if (!string.IsNullOrEmpty(header.Tag))
        {
            context.Add("Tag", header.Tag);
        }

        return context;
    }

    private static async ValueTask<IMemoryOwner<byte>> ProcessRequestBody<TRequest, TResponse>(IServiceProvider scopeProvider, string operation, ReadOnlyMemory<byte> requestBody, Func<uint, TResponse> nonOkResponseFactory, ILogger logger, CancellationToken cancellationToken)
    {
        TRequest request = MessagePackSerializer.Deserialize<TRequest>(requestBody);

        IEnumerable<IRpcPipeline<TRequest, TResponse>>? pipeline = (IEnumerable<IRpcPipeline<TRequest, TResponse>>?)scopeProvider.GetService(typeof(IEnumerable<IRpcPipeline<TRequest, TResponse>>));
        IRpcRequestHandler<TRequest, TResponse> handler = (IRpcRequestHandler<TRequest, TResponse>)(scopeProvider.GetService(typeof(IRpcRequestHandler<TRequest, TResponse>))
              ?? throw new InvalidProgramException($"RPC request handler with request type {typeof(TRequest).FullName} and response type {typeof(TResponse).FullName} not found."));

#if DEBUG
        logger.LogInformation("Executing operation {operationName} with parameters {request}.",
            operation,
            System.Text.Json.JsonSerializer.Serialize<TRequest>(request));
#endif

        try
        {
            TResponse response;
            if (pipeline == null)
            {
                response = await handler.Handle(request, cancellationToken);
            }
            else
            {
                IRpcPipeline<TRequest, TResponse>[] pipelineArray = pipeline.ToArray();
                if (pipelineArray.Length == 0)
                {
                    response = await handler.Handle(request, cancellationToken);
                }
                else
                {
                    Func<TRequest, ValueTask<TResponse>> next = r => handler.Handle(r, cancellationToken);
                    PipelineContext pipelineContext = new PipelineContext(operation, null, handler.GetType(), cancellationToken);

                    for (int i = pipelineArray.Length - 1; i >= 0; i--)
                    {
                        Func<TRequest, ValueTask<TResponse>> nextLocal = next;
                        next = (r) => pipelineArray[i].Process(pipelineContext, r, nextLocal);
                    }

                    response = await next.Invoke(request);
                }
            }

            OwnedBufferWriter bodyWriter = new OwnedBufferWriter(1024 * 8);
            MessagePackSerializer.Serialize<TResponse>(bodyWriter, response);
            return bodyWriter;
        }
        catch (RpcPkcs11Exception ex)
        {
            logger.LogError(ex, "Pkcs11 error during operation {operation} with CKRV: {ckrv}.", operation, ex.ReturnValue);

            TResponse errorResponse = nonOkResponseFactory((uint)ex.ReturnValue);

            OwnedBufferWriter bodyWriter = new OwnedBufferWriter(256);
            MessagePackSerializer.Serialize<TResponse>(bodyWriter, errorResponse);
            return bodyWriter;
        }
        catch (NotSupportedException ex)
        {
            logger.LogError(ex, "Not supported exception in operation {operation}.", operation);

            TResponse errorResponse = nonOkResponseFactory((uint)CKR.CKR_FUNCTION_NOT_SUPPORTED);
            OwnedBufferWriter bodyWriter = new OwnedBufferWriter(256);
            MessagePackSerializer.Serialize<TResponse>(bodyWriter, errorResponse);
            return bodyWriter;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "General error during operation {operation}.", operation);

            TResponse errorResponse = nonOkResponseFactory((uint)CKR.CKR_GENERAL_ERROR);
            OwnedBufferWriter bodyWriter = new OwnedBufferWriter(256);
            MessagePackSerializer.Serialize<TResponse>(bodyWriter, errorResponse);
            return bodyWriter;
        }
    }
}
