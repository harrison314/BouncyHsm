#ifdef _WIN32
#include <Windows.h>
#include <stdio.h>
#endif

#ifdef __linux__
#define __STDC_WANT_LIB_EXT2__ 1
#define _GNU_SOURCE
#include <stdio.h>
#include <syslog.h>
#include <stdarg.h>
#endif

#include "logger.h"
#include <stdlib.h>
#include <stdbool.h>
#include <string.h>

#include "platformHelper.h"
#define USE_VARIABLE(x) (void)(x)

static void send_log_message_None(int level, const char* levelText, const char* message)
{
   USE_VARIABLE(level);
   USE_VARIABLE(levelText);
   USE_VARIABLE(message);
}

static void send_log_message_console(int level, const char* levelText, const char* message)
{
	USE_VARIABLE(level);

	printf("%s: %s" NEW_LINE_STR, levelText, message);
	fflush(stdout);
}

static void send_log_message_errconsole(int level, const char* levelText, const char* message)
{
	USE_VARIABLE(level);

	fprintf(stderr, "%s: %s" NEW_LINE_STR, levelText, message);
	fflush(stderr);
}

#ifdef _WIN32
static void send_log_message_debug_static256(int level, const char* levelText, const char* message)
{
	USE_VARIABLE(level);

	char buffer[256];
	int rc = sprintf_s(buffer, sizeof(buffer), "%s: %s", levelText, message);
	if (rc == -1)
	{	
		return;
	}

	OutputDebugStringA(buffer);
}
#endif

static void send_log_message_debug(int level, const char* levelText, const char* message)
{
#ifdef _WIN32
	size_t size = (size_t)(strlen(levelText) + strlen(message) + 8);

	if (size < 256)
	{
		send_log_message_debug_static256(level, levelText, message);
		return;
	}

	char* buffer = (char*)malloc(size);
	if (buffer == NULL)
	{
		return;
	}

	int rc = sprintf_s(buffer, size, "%s: %s", levelText, message);
	if (rc == -1)
	{
		return;
	}

	OutputDebugStringA(buffer);
	free(buffer);
#else
    USE_VARIABLE(level);
    USE_VARIABLE(levelText);
    USE_VARIABLE(message);
#endif
}

static void send_log_message_syslog(int level, const char* levelText, const char* message)
{
#ifdef __linux__
    USE_VARIABLE(levelText);

	int syslogLevel = LOG_DEBUG;
	if (level == LOG_LEVEL_INFO)
	{
		syslogLevel = LOG_INFO;
	}
	else if (level == LOG_LEVEL_ERROR)
	{
		syslogLevel = LOG_ERR;
	}

	openlog("BouncyHsm.Pkcs11Lib.so", LOG_PID | LOG_CONS | LOG_ODELAY, LOG_USER);
	syslog(syslogLevel, "%s", message);
	closelog();
#endif
}

int globalLogLevelLevel = LOG_LEVEL_NONE;
send_log_message_ptr globalLogLevelTarget = send_log_message_None;

bool logger_init(const char* level, const char* target)
{
	if (level == NULL || target == NULL)
	{
		globalLogLevelLevel = LOG_LEVEL_NONE;
		globalLogLevelTarget = send_log_message_None;

		return true;
	}

	send_log_message_ptr localTarget;
	int localLevel;

	if (strcmp(level, LOG_LEVEL_NONE_NAME) == 0)
	{
		localLevel = LOG_LEVEL_NONE;
	}
	else if (strcmp(level, LOG_LEVEL_ERROR_NAME) == 0)
	{
		localLevel = LOG_LEVEL_ERROR;
	}
	else if (strcmp(level, LOG_LEVEL_INFO_NAME) == 0)
	{
		localLevel = LOG_LEVEL_INFO;
	}
	else if (strcmp(level, LOG_LEVEL_TRACE_NAME) == 0)
	{
		localLevel = LOG_LEVEL_TRACE;
	}
	else
	{
		fprintf(stderr, "Error in function logger_init level is not valid." NEW_LINE_STR);
		fflush(stderr);

		return false;
	}

	if (strcmp(target, LOG_TARGET_NONE) == 0)
	{
		localTarget = send_log_message_None;
	}
	else if (strcmp(target, LOG_TARGET_CONSOLE) == 0)
	{
		localTarget = send_log_message_console;
	}
	else if (strcmp(target, LOG_TARGET_ERR_CONSOLE) == 0)
	{
		localTarget = send_log_message_errconsole;
	}
	else if (strcmp(target, LOG_TARGET_WIN_DEBUG) == 0)
	{
		localTarget = send_log_message_debug;
	}
	else if (strcmp(target, LOG_TARGET_LINUX_SYSLOG) == 0)
	{
		localTarget = send_log_message_syslog;
	}
	else
	{
		fprintf(stderr, "Error in function logger_init target is not valid." NEW_LINE_STR);
		fflush(stderr);

		return false;
	}

	globalLogLevelLevel = localLevel;
	globalLogLevelTarget = localTarget;

	return true;
}



#ifdef _MSC_VER
int vasprintf(char** strp, const char* fmt, va_list ap) {
	// _vscprintf tells you how big the buffer needs to be
	int len = _vscprintf(fmt, ap);
	if (len == -1) {
		return -1;
	}

	char* str = (char*)malloc((size_t)len + 1);
	if (!str) {
		return -1;
	}

	//_vsprintf_p

	// _vsprintf_s is the "secure" version of vsprintf
	int r = _vsprintf_p(str, (size_t)(len + 1), fmt, ap);
	if (r == -1) {
		free(str);
		return -1;
	}
	*strp = str;
	return r;
}
#endif // _WIN32

void log_message(int level, const char* format, ...)
{
	if (level > globalLogLevelLevel)
	{
		return;
	}

	va_list ap;
	char* strp = NULL;
	va_start(ap, format);
	vasprintf(&strp, format, ap);
	va_end(ap);

	const char* levelText = "NONE";
	switch (level)
	{
	case LOG_LEVEL_ERROR:
		levelText = LOG_LEVEL_ERROR_NAME;
		break;

	case LOG_LEVEL_INFO:
		levelText = LOG_LEVEL_INFO_NAME;
		break;

	case LOG_LEVEL_TRACE:
		levelText = LOG_LEVEL_TRACE_NAME;
		break;

	default:
		levelText = "UNKNOWN";
		break;
	}

	if (strp != NULL)
	{
		globalLogLevelTarget(level, levelText, strp);
		free((void*)strp);
	}
}

bool log_level_is_enabled(int level)
{
    return (level <= globalLogLevelLevel);
}
