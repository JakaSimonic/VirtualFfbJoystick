#pragma once
#include <basetyps.h>
#include <stdlib.h>
#include <wtypes.h>
#include <initguid.h>
#include <stdio.h>
#include <string.h>
#include <conio.h>
#include <ntddkbd.h>
#include <hidclass.h>

#pragma warning(disable:4201)

#include <setupapi.h>
#include <winioctl.h>

#pragma warning(default:4201)

#include "..\VirtualHID\public.h"

#ifdef __cplusplus
extern "C" {
#endif
__declspec(dllexport)  DWORD Read_GetFeatureQueue(LPVOID buffer, int bufferSize, LPDWORD bytesReturned);
__declspec(dllexport)  DWORD Read_GetInputReportQueue(LPVOID buffer, int bufferSize, LPDWORD bytesReturned);
__declspec(dllexport)  DWORD Read_SetFeatureQueue(LPVOID buffer, int bufferSize, LPDWORD bytesReturned);
__declspec(dllexport)  DWORD Read_WriteQueue(LPVOID buffer, int bufferSize, LPDWORD bytesReturned);
__declspec(dllexport)  DWORD Read_ReadQueue(LPVOID buffer, int bufferSize, LPDWORD bytesReturned);
__declspec(dllexport)  DWORD Read_SetOutputQueue(LPVOID buffer, int bufferSize, LPDWORD bytesReturned);

__declspec(dllexport)  DWORD Write_GetFeatureQueue(LPVOID buffer, int bytesToWrite);
__declspec(dllexport)  DWORD Write_ReadQueue(LPVOID buffer, int bytesToWrite);
__declspec(dllexport)  DWORD Write_GetInputQueue(LPVOID buffer, int bytesToWrite);

__declspec(dllexport)  DWORD OpenDriverFile();
__declspec(dllexport)  DWORD CloseDriverFile();

__declspec(dllexport)  DWORD TestRead(PVOID buffer, DWORD bufferLength, LPDWORD returnedBytes);
__declspec(dllexport)  DWORD TestWrite(PVOID testSequence, DWORD sequenceLen);
#ifdef __cplusplus
}
#endif