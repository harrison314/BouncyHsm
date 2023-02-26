#ifndef PLATFORM_HELPER_H
#define PLATFORM_HELPER_H

#include<stdint.h>

#ifdef _WIN32
#include <Windows.h>


#define NEW_LINE_STR "\r\n"
#define GetCurrentPid() ((uint64_t)GetCurrentProcessId())

#endif

#ifdef __linux__
#include <sys/types.h>
#include <unistd.h>


#define NEW_LINE_STR "\n"
#define GetCurrentPid() ((uint64_t)getpid())

#endif


#endif //PLATFORM_HELPER_H