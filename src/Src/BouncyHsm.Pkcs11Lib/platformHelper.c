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

#pragma GCC diagnostic ignored "-Wstringop-overflow"
#pragma GCC diagnostic ignored "-Wstringop-truncation"
    strncpy(destination, _Source, srcSize);
#pragma GCC diagnostic push
#pragma GCC diagnostic push

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

#ifdef _WIN32

static char* LPWSTRtoUtf8(const LPWSTR src, int* out_len)
{
    size_t src_length = wcslen(src);
    int length = WideCharToMultiByte(CP_UTF8, 0, src, (int)src_length, 0, 0, NULL, NULL);
    if (length == ERROR_NO_UNICODE_TRANSLATION)
    {
        return NULL;
    }

    char* output_buffer = (char*)malloc(sizeof(char) * (length + 1));
    if (output_buffer == NULL)
    {
        return NULL;
    }

    WideCharToMultiByte(CP_UTF8, 0, src, (int)src_length, output_buffer, length, NULL, NULL);
    output_buffer[length] = 0;

    if (out_len != NULL)
    {
        *out_len = length;
    }

    return output_buffer;
}

bool getProgramArgs(const char*** args, int* argc)
{
    if (args == NULL || argc == NULL)
    {
        return false;
    }

    LPWSTR* szArgList = NULL;
    int argCount;

    szArgList = CommandLineToArgvW(GetCommandLine(), &argCount);
    if (szArgList == NULL)
    {
        // Unable to parse command line
        return false;
    }

    const char** arguments = (const char**)malloc(argCount * sizeof(char*));
    if (arguments == NULL)
    {
        if (szArgList != NULL)
        {
            LocalFree(szArgList);
        }

        return false;
    }

    for (int i = 0; i < argCount; i++)
    {
        arguments[i] = LPWSTRtoUtf8(szArgList[i], NULL);
    }

    if (szArgList != NULL)
    {
        LocalFree(szArgList);
    }

    *args = arguments;
    *argc = argCount;
    return true;
}

bool freeProgramArgs(const char*** args, int* argc)
{
    if (args == NULL || argc == NULL)
    {
        return false;
    }

    if (*args == NULL)
    {
        return true;
    }

    int i;
    int count = *argc;
    for (i = 0; i < count; i++)
    {
        const char* ptr = (*args)[i];
        if (ptr != NULL)
        {
            free((void*)ptr);
        }
    }

    free((void*)*args);
    *args = NULL;
    *argc = 0;
    return true;
}

#endif

#ifdef __linux__

bool getProgramArgs(const char*** args, int* argc)
{
    if (args == NULL || argc == NULL)
    {
        return false;
    }

    FILE* fd = fopen("/proc/self/cmdline", "rb");
    if (fd == NULL)
    {
        return false;
    }

    char* buffer = NULL;
    size_t dataSize = 0;
    char tmpBuffer[4096];
    size_t readed;

    while ((readed = fread(tmpBuffer, 1, sizeof(tmpBuffer), fd)) > 0)
    {
        char* newBuffer = (char*)realloc(buffer, dataSize + readed + 1);
        if (newBuffer == NULL)
        {
            if (buffer != NULL)
            {
                free((void*)buffer);
            }

            fclose(fd);
            return false;
        }

        buffer = newBuffer;
        memcpy(buffer + dataSize, tmpBuffer, readed);
        dataSize += readed;
        buffer[dataSize] = '\0';
    }

    fclose(fd);

    int arraySize = 0;
    size_t i = 0;
    for (i = 0; i < dataSize; i++)
    {
        if (buffer[i] == '\0')
        {
            arraySize++;
        }
    }

    const char** localArgs = (const char**)malloc(arraySize * sizeof(char*));
    if (localArgs == NULL)
    {
        if (buffer != NULL)
        {
            free((void*)buffer);
        }

        return false;
    }

    i = 0;
    int j = 0;

    while (i < dataSize)
    {
        char* dumpArgument = strdup(&buffer[i]);
        if (dumpArgument == NULL)
        {
            if (buffer != NULL)
            {
                free((void*)buffer);
            }

            if (localArgs != NULL)
            {
                int k;
                for (k = 0; k < j; k++)
                {
                    if (localArgs[k] != NULL)
                    {
                        free((void*)localArgs[k]);
                    }
                }
                free((void*)localArgs);
            }

            return false;
        }

        localArgs[j] = dumpArgument;
        j++;
        i += strlen(&buffer[i]) + 1;
    }

    free((void*)buffer);

    *args = localArgs;
    *argc = arraySize;
    return true;
}

bool freeProgramArgs(const char*** args, int* argc)
{
    if (args == NULL || argc == NULL)
    {
        return false;
    }

    if (*args == NULL)
    {
        return true;
    }

    int i;
    int count = *argc;
    for (i = 0; i < count; i++)
    {
        const char* ptr = (*args)[i];
        if (ptr != NULL)
        {
            free((void*)ptr);
        }
    }

    free((void*)*args);
    *args = NULL;
    *argc = 0;
    return true;
}

#endif