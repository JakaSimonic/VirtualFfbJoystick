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

__declspec(dllexport)  DWORD ReadGetFeatureReport(LPVOID buffer, int bufferSize, LPDWORD bytesReturned);
__declspec(dllexport)  DWORD ReadGetInputReportReport(LPVOID buffer, int bufferSize, LPDWORD bytesReturned);
__declspec(dllexport)  DWORD ReadSetFeatureReport(LPVOID buffer, int bufferSize, LPDWORD bytesReturned);
__declspec(dllexport)  DWORD ReadWriteReport(LPVOID buffer, int bufferSize, LPDWORD bytesReturned);
__declspec(dllexport)  DWORD ReadReadReport(LPVOID buffer, int bufferSize, LPDWORD bytesReturned);
__declspec(dllexport)  DWORD ReadSetOutputReport(LPVOID buffer, int bufferSize, LPDWORD bytesReturned);

__declspec(dllexport)  DWORD WriteGetFeatureReport(LPVOID buffer, int bytesToWrite);
__declspec(dllexport)  DWORD WriteReadReport(LPVOID buffer, int bytesToWrite);
__declspec(dllexport)  DWORD WriteGetInputReport(LPVOID buffer, int bytesToWrite);

__declspec(dllexport)  DWORD OpenDriverFile();
__declspec(dllexport)  DWORD CloseDriverFile();

__declspec(dllexport)  DWORD TestRead(PVOID buffer, DWORD bufferLength, LPDWORD returnedBytes);
__declspec(dllexport)  DWORD TestWrite(PVOID testSequence, DWORD sequenceLen);
