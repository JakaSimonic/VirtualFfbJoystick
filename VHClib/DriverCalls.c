/*++

Copyright (c) Jaka Simonic

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
PURPOSE.

Module Name:

DriverCalls.c

Abstract:


Environment:

usermode communication library

--*/


#include "DriverCalls.h"

//-----------------------------------------------------------------------------
// 4127 -- Conditional Expression is Constant warning
//-----------------------------------------------------------------------------
#define WHILE(constant) \
__pragma(warning(disable: 4127)) while(constant); __pragma(warning(default: 4127))

#define READ_HID(what, ioctl)																					\
		__declspec(dllexport)  DWORD Read##what##Report(LPVOID buffer, int bufferSize, LPDWORD bytesReturned)	\
		{																										\
			return ReadFromDriver(ioctl, buffer, bufferSize, bytesReturned);									\
		}

#define WRITE_HID(what, ioctl)																\
		__declspec(dllexport)  DWORD Write##what##Report(LPVOID buffer, int bytesToWrite)	\
		{																					\
			return WriteToDriver(ioctl, buffer, bytesToWrite);								\
		}



#define STATUS_PIPE_EMPTY	0xC00000D9L
#define STATUS_OK			0L
#define STATUS_NOK			-1L

HANDLE	file = NULL;

static DWORD ReadFromDriver(DWORD controlCode, LPVOID buffer, DWORD bufferSize, LPDWORD bytesRecivedSize)
{
	if (NULL == file)
	{
		printf("File not open\n");
		return (DWORD)(-1);
	}
	return DeviceIoControl(file,
		controlCode,
		NULL, 0,
		buffer, bufferSize,
		bytesRecivedSize, NULL);
}

static DWORD WriteToDriver(DWORD controlCode, LPVOID buffer, DWORD bytesToWrite)
{
	DWORD returnSize = 0;
	if (NULL == file)
	{
		printf("File not open\n");
		return -1;
	}

	if (!DeviceIoControl(file,
		controlCode,
		buffer, bytesToWrite,
		NULL, 0,
		&returnSize, NULL))
	{
		printf("Write to driver failed:0x%x\n", GetLastError());
		return -1;
	}
	return returnSize;
}

static DWORD OpenDevice(LPGUID deviceInterface, PHANDLE pFile)
{
	HDEVINFO                            hardwareDeviceInfo;
	SP_DEVICE_INTERFACE_DATA            deviceInterfaceData;
	PSP_DEVICE_INTERFACE_DETAIL_DATA    deviceInterfaceDetailData = NULL;
	ULONG                               predictedLength = 0;
	ULONG                               requiredLength = 0;
	ULONG                               i = 0;

	hardwareDeviceInfo = SetupDiGetClassDevs(
		deviceInterface,
		NULL, // Define no enumerator (global)
		NULL, // Define no
		(DIGCF_PRESENT | // Only Devices present
			DIGCF_DEVICEINTERFACE)); // Function class devices.
	if (INVALID_HANDLE_VALUE == hardwareDeviceInfo)
	{
		printf("SetupDiGetClassDevs failed: %x\n", GetLastError());
		return 0;
	}

	deviceInterfaceData.cbSize = sizeof(SP_DEVICE_INTERFACE_DATA);

	printf("\nList of KBFILTER Device Interfaces\n");
	printf("---------------------------------\n");

	i = 0;

	//
	// Enumerate devices of toaster class
	//

	do {
		if (SetupDiEnumDeviceInterfaces(hardwareDeviceInfo,
			0, // No care about specific PDOs
			deviceInterface,
			i, //
			&deviceInterfaceData)) {

			if (deviceInterfaceDetailData) {
				free(deviceInterfaceDetailData);
				deviceInterfaceDetailData = NULL;
			}

			//
			// Allocate a function class device data structure to
			// receive the information about this particular device.
			//

			//
			// First find out required length of the buffer
			//

			if (!SetupDiGetDeviceInterfaceDetail(
				hardwareDeviceInfo,
				&deviceInterfaceData,
				NULL, // probing so no output buffer yet
				0, // probing so output buffer length of zero
				&requiredLength,
				NULL)) { // not interested in the specific dev-node
				if (ERROR_INSUFFICIENT_BUFFER != GetLastError()) {
					printf("SetupDiGetDeviceInterfaceDetail failed %d\n", GetLastError());
					SetupDiDestroyDeviceInfoList(hardwareDeviceInfo);
					return FALSE;
				}

			}

			predictedLength = requiredLength;

			deviceInterfaceDetailData = malloc(predictedLength);

			if (deviceInterfaceDetailData) {
				deviceInterfaceDetailData->cbSize =
					sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA);
			}
			else {
				printf("Couldn't allocate %d bytes for device interface details.\n", predictedLength);
				SetupDiDestroyDeviceInfoList(hardwareDeviceInfo);
				return FALSE;
			}


			if (!SetupDiGetDeviceInterfaceDetail(
				hardwareDeviceInfo,
				&deviceInterfaceData,
				deviceInterfaceDetailData,
				predictedLength,
				&requiredLength,
				NULL)) {
				printf("Error in SetupDiGetDeviceInterfaceDetail\n");
				SetupDiDestroyDeviceInfoList(hardwareDeviceInfo);
				free(deviceInterfaceDetailData);
				return FALSE;
			}
			printf("%d) %s\n", ++i,
				deviceInterfaceDetailData->DevicePath);
		}
		else if (ERROR_NO_MORE_ITEMS != GetLastError()) {
			free(deviceInterfaceDetailData);
			deviceInterfaceDetailData = NULL;
			continue;
		}
		else
			break;

	} WHILE(TRUE);


	SetupDiDestroyDeviceInfoList(hardwareDeviceInfo);

	if (!deviceInterfaceDetailData)
	{
		printf("No device interfaces present\n");
		return 0;
	}


	printf("\nOpening the last interface:\n %s\n",
		deviceInterfaceDetailData->DevicePath);

	*pFile = CreateFile(deviceInterfaceDetailData->DevicePath,
		GENERIC_READ | GENERIC_WRITE,
		0,
		NULL, // no SECURITY_ATTRIBUTES structure
		OPEN_EXISTING, // No special create flags
		0, // No special attributes
		NULL);

	if (INVALID_HANDLE_VALUE == file) {
		printf("Error in CreateFile: %x", GetLastError());
		free(deviceInterfaceDetailData);
		return 0;
	}
	free(deviceInterfaceDetailData);
	printf("open complete\n");
	return 1;
	}

#ifdef __cplusplus
extern "C" {
#endif

	READ_HID(GetFeature, GET_FEATURE)
	READ_HID(SetFeature, SET_FEATURE)
	READ_HID(Write, WRITE_REPORT)
	READ_HID(Read, READ_REPORT)
	READ_HID(GetInputReport, GET_INPUT_REPORT)
	READ_HID(SetOutput, SET_OUTPUT_REPORT)

	WRITE_HID(GetFeature, COMPLETE_GET_FEATURE)
	WRITE_HID(Read, COMPLETE_READ_REPORT)
	WRITE_HID(GetInput, COMPLETE_GET_INPUT_REPORT)
	
	__declspec(dllexport)  DWORD OpenDriverFile()
	{
		return 	OpenDevice((LPGUID)&GUID_DEVINTERFACE_VIRTUALHID, &file);
	}

	__declspec(dllexport)  DWORD CloseDriverFile()
	{
		return 	CloseHandle(file);
	}

	__declspec(dllexport)  DWORD TestRead(PVOID buffer, DWORD bufferLength, LPDWORD returnedBytes)
	{
		return ReadFromDriver(TEST_READ, buffer, bufferLength, returnedBytes);
	}

	__declspec(dllexport) DWORD TestWrite(PVOID testSequence, DWORD sequenceLen)
	{
		return WriteToDriver(TEST_WRITE, testSequence, sequenceLen);
	}

#ifdef __cplusplus
}
#endif