#ifdef _WIN32
#include <Windows.h>
#else
  //NOP
#endif

#include "globalContext.h"

#ifdef _WIN32
#pragma comment(lib,"ws2_32.lib") 

BOOL APIENTRY DllMain(HMODULE hModule,
    DWORD  ul_reason_for_call,
    LPVOID lpReserved
)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        GlobalContextInit();
        break;

    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

#else

// Entry point for the shared library on unix platforms
__attribute__((constructor)) void bouncyhsm_init_entry_point(void)
{
    GlobalContextInit();
}

// Exit point for the shared library on unix platforms
__attribute__((destructor)) void bouncyhsm_init_exit_point(void)
{
    //NOP
}

#endif