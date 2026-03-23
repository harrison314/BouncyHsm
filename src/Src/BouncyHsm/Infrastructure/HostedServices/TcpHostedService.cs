using BouncyHsm.Core.Rpc;
using BouncyHsm.Services.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Infrastructure.HostedServices;

internal sealed class TcpHostedService : BackgroundService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IOptions<BouncyHsmSetup> bouncyHsmSetup;
    private readonly ILogger<TcpHostedService> logger;

    public TcpHostedService(IServiceProvider serviceProvider,
        IOptions<BouncyHsmSetup> bouncyHsmSetup,
        ILogger<TcpHostedService> logger)
    {
        this.serviceProvider = serviceProvider;
        this.bouncyHsmSetup = bouncyHsmSetup;
        this.logger = logger;

        System.Diagnostics.Debug.Assert(this.bouncyHsmSetup.Value.TcpEndpoint != null);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (this.bouncyHsmSetup.Value.TcpEndpoint == null)
        {
            this.logger.LogWarning("TCP endpoint is not enabled.");
            return;
        }

        using Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        socket.ReceiveTimeout = this.TimeSpanToTimeout(this.bouncyHsmSetup.Value.TcpEndpoint.ReceiveTimeout);
        socket.SendTimeout = this.TimeSpanToTimeout(this.bouncyHsmSetup.Value.TcpEndpoint.SendTimeout);

        socket.Bind(System.Net.IPEndPoint.Parse(this.bouncyHsmSetup.Value.TcpEndpoint.Endpoint));
        socket.Listen(100);

        this.logger.LogInformation("Starting TCP listening on {bindAddress}.", this.bouncyHsmSetup.Value.TcpEndpoint.Endpoint);

        while (!stoppingToken.IsCancellationRequested)
        {
            Socket clientConnection = await socket.AcceptAsync(stoppingToken);
            _ = this.ProcessClientConnection(clientConnection, stoppingToken);
        }
    }

    private async Task ProcessClientConnection(Socket clientConnection, CancellationToken stoppingToken)
    {
        await Task.Yield();

        using IDisposable? logScope = this.logger.BeginScope(new Dictionary<string, object>()
        {
            { "RequestId", Guid.NewGuid() }
        });

        this.logger.LogTrace("Entering to ProcessClientConnection with remote endpoint {remoteEndpoint}.", clientConnection.RemoteEndPoint);
        try
        {
            using ExactOwnedMemory headBuffer = ExactOwnedMemory.Rent(8);
            await this.ReceiveExact(clientConnection, headBuffer.Memory, stoppingToken);

            (int headerSize, int bodySize) = HeadEncoder.Decode(headBuffer.Memory.Span.Slice(0, 8));

            using ExactOwnedMemory requestHeader = ExactOwnedMemory.Rent(headerSize);
            using ExactOwnedMemory requestBody = ExactOwnedMemory.Rent(bodySize);

            await this.ReceiveExact(clientConnection, requestHeader.Memory, stoppingToken);
            await this.ReceiveExact(clientConnection, requestBody.Memory, stoppingToken);

            using (IServiceScope scope = this.serviceProvider.CreateScope())
            {
                using ResponseValue responseValue = await RequestProcessor.Process(scope.ServiceProvider,
                    requestHeader.Memory,
                    requestBody.Memory, stoppingToken);

                using ExactOwnedMemory head = ExactOwnedMemory.Rent(8);
                HeadEncoder.Encode(responseValue.Header, responseValue.Body, head.Memory);
                await clientConnection.SendAsync(head.Memory, SocketFlags.None, stoppingToken);
                await clientConnection.SendAsync(responseValue.Header, SocketFlags.None, stoppingToken);
                await clientConnection.SendAsync(responseValue.Body, SocketFlags.None, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Exception during process request.");
        }
        finally
        {
            try
            {
                clientConnection.Dispose();
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex, "Exception during close socket.");
            }
        }
    }

    private int TimeSpanToTimeout(TimeSpan? timeout)
    {
        return timeout.HasValue ? (int)timeout.Value.TotalMilliseconds : 0;
    }

    private async Task ReceiveExact(Socket socket, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        int totalRead = 0;
        while (totalRead < buffer.Length)
        {
            int bytesRead = await socket.ReceiveAsync(buffer.Slice(totalRead), SocketFlags.None, cancellationToken);
            if (bytesRead == 0)
            {
                this.logger.LogError("Read socket error with error in ReceiveExact.");
                throw new SocketException((int)SocketError.ConnectionReset, "Read socket erro in method ReceiveExact.");
            }

            totalRead += bytesRead;
        }
    }
}
