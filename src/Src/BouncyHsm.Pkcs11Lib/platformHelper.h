#ifndef PLATFORM_HELPER_H
#define PLATFORM_HELPER_H

#include<stdint.h>
#include<stdbool.h>

#ifdef _WIN32
#include <Windows.h>


#define NEW_LINE_STR "\r\n"
#define GetCurrentPid() ((uint64_t)GetCurrentProcessId())
#define _InsertBreakpoint() DebugBreak()

#endif

#ifdef __linux__
#include <sys/types.h>
#include <unistd.h>


#define NEW_LINE_STR "\n"
#define GetCurrentPid() ((uint64_t)getpid())
#define _InsertBreakpoint() __asm__("int3")

#ifdef __GNUC__
int strcpy_s(char* destination, size_t SizeInBytes, const char* _Source);
int strncpy_s(char* destination, size_t destsz, const char* _Source, size_t count);
int memcpy_s(void* restrictDest, size_t destsz, const void* restrictSrc, size_t count);
#endif

#endif

bool GetCurrentCompiuterName(char* buffer, size_t maxSize);
const char* GetPlatformName();

#endif //PLATFORM_HELPER_H