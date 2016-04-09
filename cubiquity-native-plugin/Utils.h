#pragma once
#include "CubiquityC.h"
#include <stdio.h>
#include <stdarg.h>
#include <assert.h>

// --------------------------------------------------------------------------
// Helper utilities


// COM-like Release macro
#ifndef SAFE_RELEASE
#define SAFE_RELEASE(a) if (a) { a->Release(); a = nullptr; }
#endif
#ifndef SAFE_DELETE
#define SAFE_DELETE(a) if (a) { delete a; a = nullptr; }
#endif

#ifndef V_RETURN
#define V_RETURN(x)    { hr = (x); if( FAILED(hr) ) { return false; } }
#endif

// Prints a string
inline void DebugLog(const char* str)
{
#if UNITY_WIN
	OutputDebugStringA(str);
#else
	printf("%s", str);
#endif
}

inline void LogMessage(char* pszFormat, ...)
{
	static char s_acBuf[2048]; // this here is a caveat!

	va_list args;
	va_start(args, pszFormat);
	vsprintf(s_acBuf, pszFormat, args);
	DebugLog(s_acBuf);
	va_end(args);
}

inline void validate(int returnCode)
{
	if (returnCode != CU_OK)
	{
		LogMessage("%s : %s\n", cuGetErrorCodeAsString(returnCode), cuGetLastErrorMessage());
	}
}
