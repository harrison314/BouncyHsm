#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include "globalContext.h"
#include "logger.h"

#ifndef _WIN32
#define FMT_HEADER_ONLY
//#include <fmt/format.h>
#endif // !_WIN32

#ifdef _WIN32
#include <Windows.h>
#else
#include <sys/types.h>
#include <unistd.h>
#endif // __WIN32

#include "platformHelper.h"

void myRandAddSeed(unsigned long* generator, unsigned long additionalSeed)
{
	unsigned long next = *generator;
	next = next ^ additionalSeed;
	next = next * 1103515245 + 12345;

	*generator = next;
}

unsigned int myRandNext(unsigned long* generator)
{
	unsigned long next = *generator;
	next = next * 1103515245 + 12345;
	*generator = next;

	return((unsigned int)(next / 65536) % 32768);
}

void myRandChars(unsigned long* generator, char* buffer, size_t len)
{
	size_t i;
	const char chars[] = "qwertzuiopasdfghjklyxcvbnm0123456789_";

	for (i = 0; i < len; i++)
	{
		unsigned int index = myRandNext(generator);
		buffer[i] = chars[index % (sizeof(chars) - 1)];
	}

	buffer[len] = 0;
}

#ifdef _WIN32
static char* toUtf8(const wchar_t* src, int* out_len)
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

bool GetCurrentProgramName(char* buffer, size_t maxSize)
{
	TCHAR szFileName[MAX_PATH];
	int len;

	if (GetModuleFileName(NULL, szFileName, MAX_PATH) == 0)
	{
		return false;
	}

	TCHAR* last = wcsrchr(szFileName, L'\\');
	if (last != NULL)
	{
		last++;
	}
	else
	{
		last = szFileName;
	}

	char* utfName = toUtf8(last, &len);
	if (utfName == NULL)
	{
		return false;
	}

	if (len >= maxSize)
	{
		utfName[maxSize - 1] = 0;
	}

	strcpy_s(buffer, maxSize, utfName);
	free((void*)utfName);

	return true;
}
#else
bool GetCurrentProgramName(char* buffer, size_t maxSize)
{
	FILE* procFile = fopen("/proc/self/comm", "r");
	if (procFile == NULL)
	{
		log_message(LOG_LEVEL_ERROR, "Can not open /proc/self/comm.");
		return false;
	}
	size_t readChars = fread(buffer, sizeof(char), maxSize - 1, procFile);
	buffer[readChars] = 0;
	fclose(procFile);

	return (readChars > 0);
}
#endif

bool envVariableDup(const char* name, char** variableValuePtr)
{
	if (variableValuePtr == NULL)
	{
		return false;
	}

#ifdef _WIN32
	DWORD rv = GetEnvironmentVariableA(name, NULL, 0);
	if (rv == 0)
	{
		if (GetLastError() == ERROR_ENVVAR_NOT_FOUND)
		{
			*variableValuePtr = NULL;
			return true;
		}
		else
		{
			return false;
		}
	}

	char* buffer = (char*)malloc((size_t)rv);
	if (buffer == NULL)
	{
		return false;
	}

	rv = GetEnvironmentVariableA(name, buffer, rv);
	if (rv == 0)
	{
		free((void*)buffer);
		if (GetLastError() == ERROR_ENVVAR_NOT_FOUND)
		{
			*variableValuePtr = NULL;
			return true;
		}
		else
		{
			return false;
		}
	}

	*variableValuePtr = buffer;
	return true;
#else
	char* variable = getenv(name);
	if (variable == NULL)
	{
		*variableValuePtr = NULL;
		return true;
	}
	else
	{
		char* buffer = (char*)malloc(strlen(variable) + 1);
		if (buffer == NULL)
		{
			return false;
		}

		strcpy(buffer, variable);

		*variableValuePtr = buffer;
		return true;
	}
#endif
}


// premennu prostredia ako connection string NAME=VALUE; NAME2=VALUE2;
// rozparsovat ako treba
// "Server=127.0.0.1; Port=8765; LogTarget=ErrorCosnole; LogLevel=Error; Tag=45695;"
bool parseConnectionString(const char* connectionString, const char* name, char* outValue, int* sizePtr)
{
	if (connectionString == NULL || name == NULL || sizePtr == NULL)
	{
		return false;
	}

	bool rv = false;

	size_t nameLen = strlen(name);

	char* newName = (char*)malloc(nameLen + 2);
	if (newName == NULL)
	{
		return false;
	}

	strcpy_s(newName, nameLen + 2, name);
	newName[nameLen] = '=';
	newName[nameLen + 1] = 0;

	const char* startValue = strstr(connectionString, newName);
	if (startValue == NULL)
	{
		*sizePtr = 0;
		rv = true;
	}
	else
	{
		int size = 0;
		startValue += nameLen + 1;
		while (startValue[size] != ';' && startValue[size] != 0)
		{
			size++;
		}

		if (outValue == NULL)
		{
			*sizePtr = size + 1;
			rv = true;
		}
		else
		{
			if (*sizePtr < size + 1)
			{
				rv = false;
			}
			else
			{
				strncpy_s(outValue, *sizePtr, startValue, size);
                outValue[size] = 0; // Fix strncpy
				*sizePtr = size + 1;
				rv = true;
			}
		}
	}

	free(newName);
	return rv;
}

bool parseConnectionStringOrDefault(const char* connectionString, const char* name, char* outValue, size_t outValueSize, const char* defaultValue)
{
	int size = (int)outValueSize;

	if (!parseConnectionString(connectionString, name, outValue, &size))
	{
		return false;
	}

	if (size == 0)
	{
		if (strcpy_s(outValue, outValueSize, defaultValue) != 0)
		{
			return false;
		}
	}

	return true;
}


void configureLogging(const char* configurationString)
{
	char targetBuffer[64];
	char levelBuffer[64];

	if (!parseConnectionStringOrDefault(configurationString, "LogTarget", targetBuffer, sizeof(targetBuffer), LOG_TARGET_ERR_CONSOLE))
	{
		log_message(LOG_LEVEL_ERROR, "Error during reading LogTarget.");
		return;
	}

	if (!parseConnectionStringOrDefault(configurationString, "LogLevel", levelBuffer, sizeof(levelBuffer), LOG_LEVEL_ERROR_NAME))
	{
		log_message(LOG_LEVEL_ERROR, "Error during reading LogLevel.");
		return;
	}

	if (!logger_init(levelBuffer, targetBuffer))
	{
		log_message(LOG_LEVEL_ERROR, "Error during initialized logger. Check LogTarget and LogLevel setting.");
	}
}


GlobalContext_t globalContext;

void GlobalContextInit()
{
#ifdef _WIN32
	uint64_t pid = (uint64_t)GetCurrentProcessId();
#else
	uint64_t pid = (uint64_t)getpid();
#endif

	unsigned long generator = (unsigned long)time(NULL);
	myRandAddSeed(&generator, (unsigned long)pid);

	myRandChars(&generator, globalContext.appIdRandomChars, 24);

    if (!GetCurrentProgramName(globalContext.appIdName, sizeof(globalContext.appIdName)))
    {
        strcpy_s(globalContext.appIdName, sizeof(globalContext.appIdName), "unknown");
    }

	globalContext.appId.Pid = pid;
	globalContext.appId.AppName = globalContext.appIdName;
	globalContext.appId.AppNonce = globalContext.appIdRandomChars;

    strcpy_s(globalContext.server, sizeof(globalContext.server), BOUNCY_HSM_DEFAULT_SERVER);
    strcpy_s(globalContext.tag, sizeof(globalContext.tag), "");
	globalContext.port = BOUNCY_HSM_DEFAULT_PORT;

	logger_init(LOG_LEVEL_ERROR_NAME, LOG_TARGET_ERR_CONSOLE);

	char* cfgString;
	if (!envVariableDup(BOUNCY_HSM_CFG_STRING, &cfgString))
	{
		log_message(LOG_LEVEL_ERROR, "Error during read environment variable.");
		return;
	}

	if (cfgString != NULL)
	{
		if (!parseConnectionStringOrDefault(cfgString, "Server", globalContext.server, sizeof(globalContext.server), BOUNCY_HSM_DEFAULT_SERVER))
		{
			log_message(LOG_LEVEL_ERROR, "Error during reading Server.");
			return;
		}

		char portBuffer[12];
		int portBufferSize = (int)sizeof(portBuffer);
		if (!parseConnectionString(cfgString, "Port", portBuffer, &portBufferSize))
		{
			log_message(LOG_LEVEL_ERROR, "Error during reading Port.");
			return;
		}

		if (portBufferSize != 0)
		{
			int portValue;
			#ifdef _WIN32
			int rv = sscanf_s(portBuffer, "%i", &portValue);
			#else
			int rv = sscanf(portBuffer, "%i", &portValue);
			#endif
			if (rv == 0)
			{
				log_message(LOG_LEVEL_ERROR, "Error during reading Port. Is not number.");
				return;
			}

			globalContext.port = portValue;
		}

        if (!parseConnectionStringOrDefault(cfgString, "Tag", globalContext.tag, sizeof(globalContext.tag), ""))
        {
            log_message(LOG_LEVEL_ERROR, "Error during reading Tag.");
            return;
        }

		configureLogging(cfgString);
	}

	log_message(LOG_LEVEL_INFO, "Successful initialized PKCS11 library.");
	log_message(LOG_LEVEL_INFO, "Config - pid %i AppName: %s Nonce: %s", globalContext.appId.Pid, globalContext.appId.AppName, globalContext.appId.AppNonce);
    log_message(LOG_LEVEL_INFO, "Config - Server: %s Port: %i", globalContext.server, globalContext.port);
    log_message(LOG_LEVEL_INFO, "Config - Tag: %s", globalContext.tag);
 }

