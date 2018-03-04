// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the VHCLIB_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// VHCLIB_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef VHCLIB_EXPORTS
#define VHCLIB_API __declspec(dllexport)
#else
#define VHCLIB_API __declspec(dllimport)
#endif

#include "stdafx.h"
#include <winioctl.h>
#include <iostream>
#include <Setupapi.h>
#include <INITGUID.H>
#include "..\VirtualHID\public.h"
#include <windows.h>

using namespace System;

namespace DriverCallsWrapper
{ 

public ref class  DriverCalls {
		HANDLE file;
		cli::pin_ptr<HANDLE *> p = &file;
public:
//	DriverCalls()
//	{
//		OpenDevice((LPGUID)&GUID_DEVINTERFACE_VIRTUALHID, &file);
//		return;
//	}
//	~DriverCalls()
//	{
//		CloseHandle(file);
//		return;
//	}
//
//	DWORD ReadGetFeatureReport(LPVOID buffer, int bufferSize, LPDWORD bytesReturned)
//	{
//		return ReadFromDriver(GET_FEATURE, buffer, bufferSize, bytesReturned);
//	}
//
//	DWORD ReadGetInputReportReport(LPVOID buffer, int bufferSize, LPDWORD bytesReturned)
//	{
//		return ReadFromDriver(GET_INPUT_REPORT, buffer, bufferSize, bytesReturned);
//	}
//
//	DWORD ReadSetFeatureReport(LPVOID buffer, int bufferSize, LPDWORD bytesReturned)
//	{
//		return ReadFromDriver(SET_FEATURE, buffer, bufferSize, bytesReturned);
//	}
//
//	DWORD ReadWriteReport(LPVOID buffer, int bufferSize, LPDWORD bytesReturned)
//	{
//		return ReadFromDriver(WRITE_REPORT, buffer, bufferSize, bytesReturned);
//	}
//
//	DWORD ReadReadReport(LPVOID buffer, int bufferSize, LPDWORD bytesReturned)
//	{
//		return ReadFromDriver(READ_REPORT, buffer, bufferSize, bytesReturned);
//	}
//
//	DWORD ReadSetOutputReport(LPVOID buffer, int bufferSize, LPDWORD bytesReturned)
//	{
//		return ReadFromDriver(SET_OUTPUT_REPORT, buffer, bufferSize, bytesReturned);
//	}
//
//
//
//	DWORD  WriteGetFeatureReport(LPVOID buffer, int bytesToWrite)
//	{
//		return WriteToDriver(COMPLETE_GET_FEATURE, buffer, bytesToWrite);
//	}
//
//	DWORD  WriteReadReport(LPVOID buffer, int bytesToWrite)
//	{
//		return WriteToDriver(COMPLETE_READ_REPORT, buffer, bytesToWrite);
//	}
//
//	DWORD  WriteGetInputReport(LPVOID buffer, int bytesToWrite)
//	{
//		return WriteToDriver(COMPLETE_GET_INPUT_REPORT, buffer, bytesToWrite);
//	}
//
//	DWORD OpenDriverFile()
//	{
//		return 	OpenDevice((LPGUID)&GUID_DEVINTERFACE_VIRTUALHID, &file);
//	}
//
//	void TestDriver(PVOID testSequence, LPDWORD sequenceLen)
//	{
//		DWORD storedSeqLen = *sequenceLen;
//		WriteToDriver(TEST_WRITE, testSequence, storedSeqLen);
//
//		memset(testSequence, 0, storedSeqLen);
//		*sequenceLen = 0;
//
//		ReadFromDriver(TEST_READ, testSequence, storedSeqLen, sequenceLen);
//	}
//
//private:
//
//	DWORD ReadFromDriver(DWORD controlCode, LPVOID buffer, DWORD bufferSize, LPDWORD bytesRecivedSize)
//	{
//		if (NULL == file)
//		{
//			std::cout << "File not open";
//			return (DWORD)(-1);
//		}
//		return DeviceIoControl(file,
//			controlCode,
//			NULL, 0,
//			buffer, bufferSize,
//			bytesRecivedSize, NULL);
//	}
//
//	DWORD WriteToDriver(DWORD controlCode, LPVOID buffer, DWORD bytesToWrite)
//	{
//		DWORD returnSize = 0;
//		if (NULL == file)
//		{
//			std::cout << "File not open";
//			return -1;
//		}
//
//		if (!DeviceIoControl(file,
//			controlCode,
//			buffer, bytesToWrite,
//			NULL, 0,
//			&returnSize, NULL))
//		{
//			std::cout << "Write to driver failed:0x%x\n" << GetLastError();
//			return -1;
//		}
//		return returnSize;
//	}
//
//	DWORD OpenDevice(LPGUID deviceInterface, PHANDLE pFile)
//	{
//		HDEVINFO                            hardwareDeviceInfo;
//		SP_DEVICE_INTERFACE_DATA            deviceInterfaceData;
//		PSP_DEVICE_INTERFACE_DETAIL_DATA    deviceInterfaceDetailData = NULL;
//		ULONG                               predictedLength = 0;
//		ULONG                               requiredLength = 0;
//		ULONG                               i = 0;
//
//		hardwareDeviceInfo = SetupDiGetClassDevs(
//			deviceInterface,
//			NULL, // Define no enumerator (global)
//			NULL, // Define no
//			(DIGCF_PRESENT | // Only Devices present
//				DIGCF_DEVICEINTERFACE)); // Function class devices.
//		if (INVALID_HANDLE_VALUE == hardwareDeviceInfo)
//		{
//			std::cout << "SetupDiGetClassDevs failed: %x\n" << GetLastError();
//			return 0;
//		}
//
//		deviceInterfaceData.cbSize = sizeof(SP_DEVICE_INTERFACE_DATA);
//
//		std::cout << "\nList of KBFILTER Device Interfaces\n";
//		std::cout << "---------------------------------\n";
//
//		i = 0;
//
//		//
//		// Enumerate devices of toaster class
//		//
//
//		do {
//			if (SetupDiEnumDeviceInterfaces(hardwareDeviceInfo,
//				0, // No care about specific PDOs
//				deviceInterface,
//				i, //
//				&deviceInterfaceData)) {
//
//				if (deviceInterfaceDetailData) {
//					free(deviceInterfaceDetailData);
//					deviceInterfaceDetailData = NULL;
//				}
//
//				//
//				// Allocate a function class device data structure to
//				// receive the information about this particular device.
//				//
//
//				//
//				// First find out required length of the buffer
//				//
//
//				if (!SetupDiGetDeviceInterfaceDetail(
//					hardwareDeviceInfo,
//					&deviceInterfaceData,
//					NULL, // probing so no output buffer yet
//					0, // probing so output buffer length of zero
//					&requiredLength,
//					NULL)) { // not interested in the specific dev-node
//					if (ERROR_INSUFFICIENT_BUFFER != GetLastError()) {
//						std::cout << "SetupDiGetDeviceInterfaceDetail failed %d\n" << GetLastError();
//						SetupDiDestroyDeviceInfoList(hardwareDeviceInfo);
//						return FALSE;
//					}
//
//				}
//
//				predictedLength = requiredLength;
//
//				deviceInterfaceDetailData = (PSP_DEVICE_INTERFACE_DETAIL_DATA)malloc(predictedLength);
//
//				if (deviceInterfaceDetailData) {
//					deviceInterfaceDetailData->cbSize =
//						sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA);
//				}
//				else {
//					std::cout << "Couldn't allocate %d bytes for device interface details.\n" << predictedLength;
//					SetupDiDestroyDeviceInfoList(hardwareDeviceInfo);
//					return FALSE;
//				}
//
//
//				if (!SetupDiGetDeviceInterfaceDetail(
//					hardwareDeviceInfo,
//					&deviceInterfaceData,
//					deviceInterfaceDetailData,
//					predictedLength,
//					&requiredLength,
//					NULL)) {
//					std::cout << "Error in SetupDiGetDeviceInterfaceDetail\n";
//					SetupDiDestroyDeviceInfoList(hardwareDeviceInfo);
//					free(deviceInterfaceDetailData);
//					return FALSE;
//				}
//				std::cout << ++i << ") " << (deviceInterfaceDetailData->DevicePath) << std::endl;
//			}
//			else if (ERROR_NO_MORE_ITEMS != GetLastError()) {
//				free(deviceInterfaceDetailData);
//				deviceInterfaceDetailData = NULL;
//				continue;
//			}
//			else
//				break;
//
//		} while (TRUE);
//
//
//		SetupDiDestroyDeviceInfoList(hardwareDeviceInfo);
//
//		if (!deviceInterfaceDetailData)
//		{
//			std::cout << "No device interfaces present\n";
//			return 0;
//		}
//
//
//		std::cout << "\nOpening the last interface:\n %s\n" <<
//			deviceInterfaceDetailData->DevicePath;
//
//		*pFile = CreateFile(deviceInterfaceDetailData->DevicePath,
//			GENERIC_READ | GENERIC_WRITE,
//			0,
//			NULL, // no SECURITY_ATTRIBUTES structure
//			OPEN_EXISTING, // No special create flags
//			0, // No special attributes
//			NULL);
//
//		if (INVALID_HANDLE_VALUE == file) {
//			std::cout << "Error in CreateFile: %x", GetLastError();
//			free(deviceInterfaceDetailData);
//			return 0;
//		}
//		free(deviceInterfaceDetailData);
//		std::cout << "open complete\n";
//		return 1;
//	}

	DriverCalls(void);
	~DriverCalls(void);

	DWORD ReadGetFeatureReport(LPVOID buffer, int bufferSize, LPDWORD bytesReturned);
	DWORD ReadGetInputReportReport(LPVOID buffer, int bufferSize, LPDWORD bytesReturned);
	DWORD ReadSetFeatureReport(LPVOID buffer, int bufferSize, LPDWORD bytesReturned);
	DWORD ReadWriteReport(LPVOID buffer, int bufferSize, LPDWORD bytesReturned);
	DWORD ReadReadReport(LPVOID buffer, int bufferSize, LPDWORD bytesReturned);
	DWORD ReadSetOutputReport(LPVOID buffer, int bufferSize, LPDWORD bytesReturned);

	DWORD WriteGetFeatureReport(LPVOID buffer, int bytesToWrite);
	DWORD WriteReadReport(LPVOID buffer, int bytesToWrite);
	DWORD WriteGetInputReport(LPVOID buffer, int bytesToWrite);

	DWORD OpenDriverFile();

	void TestDriver(PVOID testSequence, LPDWORD sequenceLen);

private:

	 DWORD ReadFromDriver(DWORD controlCode, LPVOID buffer, DWORD bufferSize, LPDWORD bytesRecivedSize);
	 DWORD WriteToDriver(DWORD controlCode, LPVOID buffer, DWORD bytesToWrite);
	 DWORD OpenDevice(LPGUID deviceInterface, cli::pin_ptr<HANDLE *> pFile);
};

}

//ref class DriverCallsWrapper
//{
//private:
//	DriverCalls *_dc;
//public:
//	DriverCallsWrapper()
//	{
//		_dc = new DriverCalls()
//	}
//	~DriverCallsWrapper()
//	{
//		delete _dc;
//		_dc = 0;
//	}
//
//	property String ^Name
//	{
//		String ^get()
//		{
//			return gcnew String(_dc->getName());
//		}
//	}
//	property double Gpa
//	{
//		double get()
//		{
//			return _dc->getGpa();
//		}
//	}
//};