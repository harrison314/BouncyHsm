#ifndef GLOBAL_CONTEXT
#define GLOBAL_CONTEXT

#include <stdint.h>
#include "rpc/rpc.h"


typedef struct _globalContext {
	char appIdRandomChars[25];
	char appIdName[50];

	AppIdentification appId;

	char server[64];
	int port;

} GlobalContext_t;

extern GlobalContext_t globalContext;

void GlobalContextInit();


#define BOUNCY_HSM_CFG_STRING "BOUNCY_HSM_CFG_STRING"
#define BOUNCY_HSM_DEFAULT_SERVER "127.0.0.1"
#define BOUNCY_HSM_DEFAULT_PORT 8765
#define PKCS11_LIB_DESCRIPTION "BouncyHsm.Pkcs11 library"

#endif //GLOBAL_CONTEXT