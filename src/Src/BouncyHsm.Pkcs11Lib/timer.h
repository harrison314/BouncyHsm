//#ifndef TIMER_H
//#define TIMER_H
//
//#include <Windows.h>
//#include <stdbool.h>
//
//typedef void (*TimerCallbackFn_t)(void* userData);
//
//typedef struct _PeriodicTimer
//{
//	HANDLE hTimer;
//	HANDLE hTimerQueue;
//	bool isInitialized;
//
//	TimerCallbackFn_t callbackPtr;
//	void* userDataPtr;
//
//} PeriodicTimer_t;
//
//bool PeriodicTimer_Create(PeriodicTimer_t* timer, TimerCallbackFn_t callbackPtr, void* userDataPtr, int periodInMs);
//bool PeriodicTimer_Destroy(PeriodicTimer_t* timer);
//#endif //TIMER_H
