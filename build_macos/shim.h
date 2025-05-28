#ifndef MACOS_SHIM_H
#define MACOS_SHIM_H

#include <stdio.h>
#include <stdarg.h>
#include <string.h>
#include <errno.h>
#include <unistd.h>   // gethostname
#include <stdbool.h>

// Define newline macro for cross-platform compatibility
#define NEW_LINE_STR "\n"

// Emulate Microsoft-secure functions using standard C/POSIX APIs
#ifndef strcpy_s
#define strcpy_s(dest, destsz, src)                                     \
    (((size_t)strlen(src) + 1 > (destsz))                               \
         ? (errno = EINVAL)                                             \
         : memcpy((dest), (src), strlen(src) + 1))
#endif

#ifndef strncpy_s
#define strncpy_s(dest, destsz, src, count)                             \
    do {                                                                \
        size_t _len = strnlen((src), (count));                          \
        if (_len + 1 > (destsz)) {                                      \
            errno = EINVAL;                                             \
        } else {                                                        \
            memcpy((dest), (src), _len);                                \
            (dest)[_len] = '\0';                                        \
        }                                                               \
    } while (0)
#endif

#ifndef memcpy_s
#define memcpy_s(dest, destsz, src, count)                              \
    (((count) > (destsz))                                               \
         ? (errno = EINVAL)                                             \
         : memcpy((dest), (src), (count)))
#endif

/**
 * Provide GetCurrentCompiuterName for macOS
 * (matches platformHelper.h signature)
 */
static inline bool GetCurrentCompiuterName(char *buffer, size_t maxSize)
{
    if (buffer == NULL || maxSize == 0) {
        return false;
    }

    if (gethostname(buffer, maxSize) != 0) {
        return false;
    }

    buffer[maxSize - 1] = '\0';
    return true;
}

#endif
