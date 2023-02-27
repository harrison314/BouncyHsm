//#include<Windows.h>
//#include <stdbool.h>
//#include "timer.h"
//
//VOID CALLBACK TimerRoutine(PVOID lpParam, BOOLEAN TimerOrWaitFired)
//{
//	if (lpParam == NULL)
//	{
//		//printf("TimerRoutine lpParam is NULL\n");
//		return;
//	}
//
//	PeriodicTimer_t* timer = (PeriodicTimer_t*)lpParam;
//
//	if (TimerOrWaitFired)
//	{
//		timer->callbackPtr(timer->userDataPtr);
//	}
//}
//
//bool PeriodicTimer_Create(PeriodicTimer_t* timer, TimerCallbackFn_t callbackPtr, void* userDataPtr, int periodInMs)
//{
//	timer->hTimer = NULL;
//	timer->callbackPtr = callbackPtr;
//	timer->userDataPtr = userDataPtr;
//
//	timer->hTimerQueue = CreateTimerQueue();
//	if (timer->hTimerQueue == NULL)
//	{
//		// printf("CreateTimerQueue failed (%d)\n", GetLastError());
//		return false;
//	}
//
//	BOOL rv = CreateTimerQueueTimer(&timer->hTimer,
//		&timer->hTimerQueue,
//		(WAITORTIMERCALLBACK)TimerRoutine,
//		(PVOID)timer,
//		(DWORD)periodInMs,
//		(DWORD)periodInMs,
//		0);
//
//	if (!rv)
//	{
//		//printf("CreateTimerQueueTimer failed (%d)\n", GetLastError());
//		return false;
//	}
//
//	timer->isInitialized = true;
//	return true;
//}
//
//bool PeriodicTimer_Destroy(PeriodicTimer_t* timer)
//{
//	if (timer->isInitialized)
//	{
//		if (!DeleteTimerQueue(timer->hTimerQueue))
//		{
//			//printf("DeleteTimerQueue failed (%d)\n", GetLastError());
//			return false;
//		}
//
//		timer->isInitialized = false;
//
//		timer->callbackPtr = NULL;
//		timer->hTimer = NULL;
//		timer->hTimerQueue = NULL;
//		timer->userDataPtr = NULL;
//
//	}
//
//	return true;
//}