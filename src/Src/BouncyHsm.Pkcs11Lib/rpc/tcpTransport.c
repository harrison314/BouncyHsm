#define WIN32_LEAN_AND_MEAN

#include <stdlib.h>
#include <string.h>

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
#include <errno.h>
#endif

#include "../rpc/rpc.h"
#include "tcpTransport.h"
#include "../logger.h"

#define USE_VARIABLE(x) (void)(x)

#ifdef _WIN32
static int isGlobalInit = 0;
#define WSA_ERROR_MESSAGE_BUFFER_LEN 320

int translateHostName(const char* hostName, int port, SockContext_t* ctx)
{
    struct addrinfo hints;
    int result;
    char portStr[6];
    sprintf_s(portStr, sizeof(portStr), "%d", port);

    ZeroMemory(&hints, sizeof(hints));
    hints.ai_family = AF_UNSPEC; // IPv4 or IPv6
    hints.ai_socktype = SOCK_STREAM; // TCP socket
    hints.ai_protocol = IPPROTO_TCP;

    ctx->addr = NULL;

    result = getaddrinfo(hostName, portStr, &hints, &ctx->addr);
    if (result != 0)
    {
        log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) getaddrinfo returns: %d", __FUNCTION__, __LINE__, result);
        return NMRPC_FATAL_ERROR;
    }

    if (ctx->addr == NULL)
    {
        log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) hostname %s can not translate to IP", __FUNCTION__, __LINE__, hostName);
        return NMRPC_FATAL_ERROR;
    }

    if (log_level_is_enabled(LOG_LEVEL_TRACE))
    {
        struct addrinfo* ptr = ctx->addr;
        while (ptr != NULL)
        {
            if (ptr->ai_family == AF_INET)
            {
                struct sockaddr_in* ipv4 = (struct sockaddr_in*)ptr->ai_addr;
                inet_ntop(AF_INET, &(ipv4->sin_addr), ctx->ipAddress, INET_ADDRSTRLEN);

                log_message(LOG_LEVEL_TRACE, "In %s: translate hostName %s to IPv4: %s",
                    __FUNCTION__,
                    hostName,
                    ctx->ipAddress);
            }

            if (ptr->ai_family == AF_INET6)
            {
                struct sockaddr_in6* ipv6 = (struct sockaddr_in6*)ptr->ai_addr;
                inet_ntop(AF_INET6, &(ipv6->sin6_addr), ctx->ipAddress, INET6_ADDRSTRLEN);

                log_message(LOG_LEVEL_TRACE, "In %s: translate hostName %s to IPv6: %s",
                    __FUNCTION__,
                    hostName,
                    ctx->ipAddress);
            }

            ptr = ptr->ai_next;

            break;
        }
    }

    return NMRPC_OK;
}

bool toUtf8Msg(wchar_t* inBuffer, char* outBuffer, size_t outBufferLen)
{
    size_t inBuffer_length = wcslen(inBuffer);
    int length = WideCharToMultiByte(CP_UTF8, 0, inBuffer, (int)inBuffer_length, 0, 0, NULL, NULL);
    if (length == ERROR_NO_UNICODE_TRANSLATION)
    {
        return false;
    }

    if (length > outBufferLen)
    {
        return false;
    }

    WideCharToMultiByte(CP_UTF8, 0, inBuffer, (int)inBuffer_length, outBuffer, length, NULL, NULL);
    outBuffer[length] = 0;

    return true;
}

int getWsaLastErrorMessage(char* buffer, size_t bufferLen)
{
    int wsaErr = WSAGetLastError();
    log_message(LOG_LEVEL_INFO, "Socket error - WSA last error: %d", wsaErr);

    wchar_t msgbuf[WSA_ERROR_MESSAGE_BUFFER_LEN];
    msgbuf[0] = L'\0';

    FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,   // flags
        NULL,                // lpsource
        wsaErr,                 // message id
        MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),    // languageid
        msgbuf,              // output buffer
        sizeof(msgbuf),     // size of msgbuf, bytes
        NULL);

    if (msgbuf[0] == L'\0')
    {
        sprintf_s(buffer, bufferLen, "%d", wsaErr);
    }
    else
    {
        char tmpBuff[WSA_ERROR_MESSAGE_BUFFER_LEN];
        if (toUtf8Msg(msgbuf, tmpBuff, sizeof(tmpBuff)))
        {
            sprintf_s(buffer, bufferLen, "%s (%d)", tmpBuff, wsaErr);
        }
        else
        {
            sprintf_s(buffer, bufferLen, "%d", wsaErr);
        }
    }

    return wsaErr;
}

int SockContext_init(SockContext_t* ctx, const char* host, int port)
{
    log_message(LOG_LEVEL_TRACE, "Init socket context with host: %s port: %d", host, port);

    if (!isGlobalInit)
    {
        WSADATA wsa;
        isGlobalInit = 1;
        if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0)
        {
            char errorMsgBuffer[WSA_ERROR_MESSAGE_BUFFER_LEN];
            getWsaLastErrorMessage(errorMsgBuffer, sizeof(errorMsgBuffer));

            log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) WSA error: %s", __FUNCTION__, __LINE__, errorMsgBuffer);
            return NMRPC_FATAL_ERROR;
        }
    }

    ctx->isInitialized = 0;

    if (translateHostName(host, port, ctx) != NMRPC_OK)
    {
        log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) can not translate hostname", __FUNCTION__, __LINE__);
        return NMRPC_FATAL_ERROR;
    }

    return NMRPC_OK;
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
        if ((ctx->s = socket(ctx->addr->ai_family, ctx->addr->ai_socktype, ctx->addr->ai_protocol)) == INVALID_SOCKET)
        {
            char errorMsgBuffer[WSA_ERROR_MESSAGE_BUFFER_LEN];
            getWsaLastErrorMessage(errorMsgBuffer, sizeof(errorMsgBuffer));

            log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) - Could not create socket. IP: %s, WSA error: %s", __FUNCTION__, __LINE__, ctx->ipAddress, errorMsgBuffer);
            return NMRPC_FATAL_ERROR;
        }

        if (connect(ctx->s, ctx->addr->ai_addr, (int)ctx->addr->ai_addrlen) < 0)
        {
            char errorMsgBuffer[WSA_ERROR_MESSAGE_BUFFER_LEN];
            getWsaLastErrorMessage(errorMsgBuffer, sizeof(errorMsgBuffer));

            log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) - Connection error. IP: %s, WSA error: %s", __FUNCTION__, __LINE__, ctx->ipAddress, errorMsgBuffer);
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
        char errorMsgBuffer[WSA_ERROR_MESSAGE_BUFFER_LEN];
        getWsaLastErrorMessage(errorMsgBuffer, sizeof(errorMsgBuffer));

        log_message(LOG_LEVEL_ERROR, "shutdown failed : %s", errorMsgBuffer);
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
        char errorMsgBuffer[WSA_ERROR_MESSAGE_BUFFER_LEN];
        getWsaLastErrorMessage(errorMsgBuffer, sizeof(errorMsgBuffer));

        log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) - Received failed. WSA error %s", __FUNCTION__, __LINE__, errorMsgBuffer);
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
            char errorMsgBuffer[WSA_ERROR_MESSAGE_BUFFER_LEN];
            getWsaLastErrorMessage(errorMsgBuffer, sizeof(errorMsgBuffer));

            log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) - Close socket. WSA error %s", __FUNCTION__, __LINE__, errorMsgBuffer);
        }
    }

    if (ctx->addr != NULL)
    {
        freeaddrinfo(ctx->addr);
        ctx->addr = NULL;
    }

    return NMRPC_OK;
}

#else

int translateHostName(const char* hostName, int port, SockContext_t* ctx)
{
    struct addrinfo hints;
    int result;
    char portStr[6];
    sprintf(portStr, "%d", port);

    memset(&hints, 0, sizeof(hints));
    hints.ai_family = AF_UNSPEC; // IPv4 or IPv6
    hints.ai_socktype = SOCK_STREAM; // TCP socket
    hints.ai_protocol = IPPROTO_TCP;

    result = getaddrinfo(hostName, portStr, &hints, &ctx->addr);
    if (result != 0)
    {
        log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) getaddrinfo returns: %d", __FUNCTION__, __LINE__, result);
        return NMRPC_FATAL_ERROR;
    }

    if (ctx->addr == NULL)
    {
        log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) hostname %s can not translate to IP", __FUNCTION__, __LINE__, hostName);
        return NMRPC_FATAL_ERROR;
    }

    if (log_level_is_enabled(LOG_LEVEL_TRACE))
    {
        struct addrinfo* ptr = ctx->addr;
        while (ptr != NULL)
        {

            if (ptr->ai_family == AF_INET)
            {
                struct sockaddr_in* ipv4 = (struct sockaddr_in*)ptr->ai_addr;
                inet_ntop(AF_INET, &(ipv4->sin_addr), ctx->ipAddress, INET_ADDRSTRLEN);

                log_message(LOG_LEVEL_TRACE, "In %s: translate hostName %s to IPv4: %s",
                    __FUNCTION__,
                    hostName,
                    ctx->ipAddress);
            }

            if (ptr->ai_family == AF_INET6)
            {
                struct sockaddr_in6* ipv6 = (struct sockaddr_in6*)ptr->ai_addr;
                inet_ntop(AF_INET6, &(ipv6->sin6_addr), ctx->ipAddress, INET6_ADDRSTRLEN);

                log_message(LOG_LEVEL_TRACE, "In %s: translate hostName %s to IPv6: %s",
                    __FUNCTION__,
                    hostName,
                    ctx->ipAddress);
            }

            ptr = ptr->ai_next;

            break;
        }
    }

    return NMRPC_OK;
}

int SockContext_init(SockContext_t* ctx, const char* host, int port)
{
    log_message(LOG_LEVEL_TRACE, "Init socket context with host: %s port: %d", host, port);

    ctx->isInitialized = 0;
    ctx->s = -1;
    if (translateHostName(host, port, ctx) != NMRPC_OK)
    {
        log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) can not translate hostname", __FUNCTION__, __LINE__);
        return NMRPC_FATAL_ERROR;

    }

    return NMRPC_OK;
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
        if ((ctx->s = socket(ctx->addr->ai_family, ctx->addr->ai_socktype, ctx->addr->ai_protocol)) < 0)
        {
            log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) - Could not create socket. IP: %s Error: %s", __FUNCTION__, __LINE__, ctx->ipAddress, strerror(errno));
            return NMRPC_FATAL_ERROR;
        }

        if (connect(ctx->s, ctx->addr->ai_addr, (int)ctx->addr->ai_addrlen) < 0)
        {
            log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) - Connection error. IP: %s Error: %s", __FUNCTION__, __LINE__, ctx->ipAddress, strerror(errno));
            return NMRPC_FATAL_ERROR;
        }

        ctx->isInitialized = 1;
    }

    if (send(ctx->s, (const char*)request_data, (int)request_data_size, 0) < 0)
    {
        log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) - Connection error. Error: %s", __FUNCTION__, __LINE__, strerror(errno));
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
        log_message(LOG_LEVEL_ERROR, "setsockopt failed. Error: %s", strerror(errno));
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
        log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) - Received failed. Error: %s", __FUNCTION__, __LINE__, strerror(errno));
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
            log_message(LOG_LEVEL_ERROR, "Error in %s (line %d) - Close socket. Error: %s", __FUNCTION__, __LINE__, strerror(errno));
        }
    }

    if (ctx->addr != NULL)
    {
        freeaddrinfo(ctx->addr);
    }

    return NMRPC_OK;
}

#endif