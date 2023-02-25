#ifndef LOGGER_H
#define LOGGER_H

#include <stdbool.h>

#define LOG_LEVEL_NONE 0
#define LOG_LEVEL_ERROR 2
#define LOG_LEVEL_INFO 4
#define LOG_LEVEL_TRACE 8

#define LOG_LEVEL_NONE_NAME "NONE"
#define LOG_LEVEL_ERROR_NAME "ERROR"
#define LOG_LEVEL_INFO_NAME "INFO"
#define LOG_LEVEL_TRACE_NAME "TRACE"

#define LOG_TARGET_NONE "NONE"
#define LOG_TARGET_CONSOLE "Console"
#define LOG_TARGET_ERR_CONSOLE "ErrorConsole"
#define LOG_TARGET_WIN_DEBUG "WinDebugOut"
#define LOG_TARGET_LINUX_SYSLOG "LinuxSyslog"

typedef void (*send_log_message_ptr)(int level, const char* levelText, const char* message);

bool logger_init(const char* level, const char* target);
void log_message(int level, const char* format, ...);

#define LOG_ENTERING_TO_FUNCTION() log_message(LOG_LEVEL_INFO, "Entering to function %s", __FUNCTION__)

#endif //LOGGER_H