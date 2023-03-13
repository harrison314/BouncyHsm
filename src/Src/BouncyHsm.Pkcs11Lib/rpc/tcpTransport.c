#define WIN32_LEAN_AND_MEAN
#ifdef _WIN32
#define _WINSOCK_DEPRECATED_NO_WARNINGS

#include <WinSock2.h>
#include <WS2tcpip.h>
#include <Windows.h>
#include <stdio.h>
#else
#include <arpa/inet.h>
#include <netinet/in.h>
#include <sys/socket.h>
#include <unistd.h>
#include <stdio.h>
#include <netinet/tcp.h>
#endif


#include "../rpc/rpc.h"
#include "tcpTransport.h"
#include "../logger.h"

#define USE_VARIABLE(x) (void)(x)

#ifdef _WIN32
static int isGlobalInit = 0;

void SockContext_init(SockContext_t* ctx, const char* host, int port)
{
    log_message(LOG_LEVEL_TRACE, "Init socket context with host: %s port: %d", host, port);

    if (!isGlobalInit)
    {
        WSADATA wsa;
        isGlobalInit = 1;
        if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0)
        {
            log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) WSA error: %d", __FUNCTION__, __LINE__, WSAGetLastError());
            return;
        }
    }

    ctx->isInitialized = 0;
    ctx->server.sin_addr.s_addr = inet_addr(host);
    ctx->server.sin_family = AF_INET;
    ctx->server.sin_port = htons(port);
}

int sock_writerequest(void* user_ctx, void* request_data, size_t request_data_size)
{
    log_message(LOG_LEVEL_TRACE, "Entering to %s : request_data: %p request_data_size: %i",
        __FUNCTION__,
        request_data,
        (int)request_data_size);

    SockContext_t* ctx = (SockContext_t*)user_ctx;

    if (!ctx->isInitialized)
    {
        if ((ctx->s = socket(AF_INET, SOCK_STREAM, 0)) == INVALID_SOCKET)
        {
            log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) - Could not create socket. WSA error: %d", __FUNCTION__, __LINE__, WSAGetLastError());
            return NMRPC_FATAL_ERROR;
        }


        if (connect(ctx->s, (struct sockaddr*)&(ctx->server), sizeof(struct sockaddr)) < 0)
        {
            log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) - Connection error.", __FUNCTION__, __LINE__);
            return NMRPC_FATAL_ERROR;
        }

        ctx->isInitialized = 1;
    }

    if (send(ctx->s, (const char*)request_data, (int)request_data_size, 0) < 0)
    {
        log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) - Connection error.", __FUNCTION__, __LINE__);
        return NMRPC_FATAL_ERROR;
    }

    return NMRPC_OK;
}

int sock_flush(void* user_ctx)
{
    log_message(LOG_LEVEL_TRACE, "Entering to %s", __FUNCTION__);

    SockContext_t* ctx = (SockContext_t*)user_ctx;

    if (shutdown(ctx->s, SD_SEND) == SOCKET_ERROR)
    {
        log_message(LOG_LEVEL_ERROR, "shutdown failed : %d", WSAGetLastError());
        return NMRPC_FATAL_ERROR;
    }

    return NMRPC_OK;
}

size_t sock_readresponse(void* user_ctx, void* response_data, size_t response_data_size)
{
    log_message(LOG_LEVEL_TRACE, "Entering to %s response_data: %p response_data_size: %i",
        __FUNCTION__,
        response_data,
        (int)response_data_size);

    SockContext_t* ctx = (SockContext_t*)user_ctx;

    int recv_size;

    //Receive a reply from the server
    if ((recv_size = recv(ctx->s, response_data, (int)response_data_size, MSG_WAITALL)) == SOCKET_ERROR)
    {
        log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) - Received failed. WSA error %d", __FUNCTION__, __LINE__, WSAGetLastError());
        return 0;
    }

    return (size_t)recv_size;
}

int readclose(void* user_ctx)
{
    log_message(LOG_LEVEL_TRACE, "Entering to %s", __FUNCTION__);

    USE_VARIABLE(user_ctx);

    SockContext_t* ctx = (SockContext_t*)user_ctx;

    if (ctx->isInitialized)
    {
        int rv = closesocket(ctx->s);
        if (rv != 0)
        {
            log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) - Close socket. WSA error %d", __FUNCTION__, __LINE__, WSAGetLastError());
        }
    }

    return NMRPC_OK;
}

#else

void SockContext_init(SockContext_t* ctx, const char* host, int port)
{
    log_message(LOG_LEVEL_TRACE, "Init socket context with host: %s port: %d", host, port);

    ctx->isInitialized = 0;
    ctx->s = -1;
    ctx->server.sin_addr.s_addr = inet_addr(host);
    ctx->server.sin_family = AF_INET;
    ctx->server.sin_port = htons(port);
}


int sock_writerequest(void* user_ctx, void* request_data, size_t request_data_size)
{
    log_message(LOG_LEVEL_TRACE, "Entering to %s : request_data: %p request_data_size: %i",
        __FUNCTION__,
        request_data,
        (int)request_data_size);

    SockContext_t* ctx = (SockContext_t*)user_ctx;

    if (!ctx->isInitialized)
    {
        if ((ctx->s = socket(AF_INET, SOCK_STREAM, 0)) < 0)
        {
            log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) - Could not create socket.", __FUNCTION__, __LINE__);
            return NMRPC_FATAL_ERROR;
        }


        if (connect(ctx->s, (struct sockaddr*)&(ctx->server), sizeof(struct sockaddr)) < 0)
        {
            log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) - Connection error.", __FUNCTION__, __LINE__);
            return NMRPC_FATAL_ERROR;
        }

        ctx->isInitialized = 1;
    }

    if (send(ctx->s, (const char*)request_data, (int)request_data_size, 0) < 0)
    {
        log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) - Connection error.", __FUNCTION__, __LINE__);
        return NMRPC_FATAL_ERROR;
    }

    return NMRPC_OK;
}

int sock_flush(void* user_ctx)
{
    log_message(LOG_LEVEL_TRACE, "Entering to %s", __FUNCTION__);

    SockContext_t* ctx = (SockContext_t*)user_ctx;

    int flag = 1;
    if (setsockopt(ctx->s, IPPROTO_TCP, TCP_NODELAY, (char*)&flag, sizeof(int)) < 0)
    {
        log_message(LOG_LEVEL_ERROR, "setsockopt failed");
        return NMRPC_FATAL_ERROR;
    }

    return NMRPC_OK;
}

size_t sock_readresponse(void* user_ctx, void* response_data, size_t response_data_size)
{
    log_message(LOG_LEVEL_TRACE, "Entering to %s response_data: %p response_data_size: %i",
        __FUNCTION__,
        response_data,
        (int)response_data_size);

    SockContext_t* ctx = (SockContext_t*)user_ctx;

    int recv_size;

    //Receive a reply from the server
    if ((recv_size = recv(ctx->s, response_data, (int)response_data_size, MSG_WAITALL)) < 0)
    {
        log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) - Received failed.", __FUNCTION__, __LINE__);
        return 0;
    }

    return (size_t)recv_size;
}

int readclose(void* user_ctx)
{
    log_message(LOG_LEVEL_TRACE, "Entering to %s", __FUNCTION__);

    SockContext_t* ctx = (SockContext_t*)user_ctx;
    if (ctx->isInitialized)
    {
        int rv = close(ctx->s);
        if (rv != 0)
        {
            log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) - Close socket.", __FUNCTION__, __LINE__);
        }
    }

    return NMRPC_OK;
}


#endif