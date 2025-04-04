#include "platformHelper.h"
#include<stdint.h>
#include<string.h>
#include<stdio.h>
#include<stdlib.h>

#ifdef __linux__
#include <sys/types.h>
#include <unistd.h>

#ifdef __GNUC__

#define _GCC_EINVAL 22
#define _GCC_ERANGE 34
int strcpy_s(char* destination, size_t SizeInBytes, const char* _Source)
{
    if (destination == NULL) return _GCC_EINVAL;
    if (SizeInBytes == 0) return _GCC_ERANGE;
    if (_Source == NULL)
    {
        destination[0] = 0;
        return _GCC_EINVAL;
    }

    size_t srclLen = strlen(_Source);
    if (srclLen > SizeInBytes)
    {
        destination[0] = 0;
        return _GCC_ERANGE;
    }

    strcpy(destination, _Source);
    return 0;
}

int strncpy_s(char* destination, size_t destsz, const char* _Source, size_t count)
{
    if (destination == NULL) return _GCC_EINVAL;
    if (destsz == 0) return _GCC_ERANGE;
    if (_Source == NULL)
    {
        destination[0] = 0;
        return _GCC_EINVAL;
    }

    size_t srcSize = strlen(_Source);
    if (srcSize > count)
    {
        srcSize = count;
    }

    if (srcSize > destsz)
    {
        destination[0] = 0;
        return _GCC_ERANGE;
    }

    strncpy(destination, _Source, srcSize);
    return 0;
}

int memcpy_s(void* restrictDest, size_t destsz, const void* restrictSrc, size_t count)
{
    if (restrictDest == NULL || restrictSrc == NULL) return _GCC_EINVAL;
    if (count > destsz) return _GCC_EINVAL;

    memcpy(restrictDest, restrictSrc, count);
    return 0;
}

#undef _GCC_EINVAL
#undef _GCC_ERANGE

#endif

bool GetCurrentCompiuterName(char* buffer, size_t maxSize)
{
    int result = gethostname(buffer, maxSize);
    return result == 0;
}

#endif

#ifdef _WIN32

#include <Windows.h>

bool GetCurrentCompiuterName(char* buffer, size_t maxSize)
{
    DWORD size = (DWORD)maxSize;
    return (bool)GetComputerNameA(buffer, &size);
}
#endif


#define MACRO_EXPLODE_STR(s) X_MACRO_EXPLODE_STR(s)
#define X_MACRO_EXPLODE_STR(s) #s

const char* GetPlatformName()
{
#if defined(BUILD_ENV)
    return MACRO_EXPLODE_STR(BUILD_ENV);
#elif  defined(_WIN32) || defined(__CYGWIN__)
    return  "Windows";
#elif defined(__linux__)
    return  "Linux";
#elif defined(__APPLE__) && defined(__MACH__)
    return  "MacOS";
#elif defined(__FreeBSD__)
    return "FreeBSD";
#elif defined(unix) || defined(__unix__) || defined(__unix)
    return  "Unix";
#else
    return  "Unknown";
#endif
}