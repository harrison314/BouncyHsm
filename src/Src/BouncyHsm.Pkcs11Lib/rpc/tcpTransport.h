#ifndef TCP_TRANSPORT_HEADER
#define TCP_TRANSPORT_HEADER


#define WIN32_LEAN_AND_MEAN
#include <WinSock2.h>
#include <WS2tcpip.h>
#include <Windows.h>

int sock_writerequest(void* user_ctx, void* request_data, size_t request_data_size);
int sock_flush(void* user_ctx);
size_t sock_readresponse(void* user_ctx, void* response_data, size_t response_data_size);
int readclose(void* user_ctx);


typedef struct _sockContext {
    SOCKET s;
    struct sockaddr_in server;
    int isInitialized;


} SockContext_t;

void SockContext_init(SockContext_t* ctx, const char* host, int port);

#define nmrpc_global_context_tcp_init(ctxPtr, socket_dataPtr) nmrpc_global_context_init((ctxPtr),(socket_dataPtr), &sock_writerequest, &sock_readresponse, &readclose, NULL)

#endif //TCP_TRANSPORT_HEADER