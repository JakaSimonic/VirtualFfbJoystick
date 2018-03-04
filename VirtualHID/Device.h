/*++

Module Name:

	device.h

Abstract:

	This file contains the device definitions.

Environment:

	Kernel-mode Driver Framework

--*/

EXTERN_C_START

typedef UCHAR HID_REPORT_DESCRIPTOR, *PHID_REPORT_DESCRIPTOR;

DRIVER_INITIALIZE                   DriverEntry;
EVT_WDF_DRIVER_DEVICE_ADD           EvtDeviceAdd;
EVT_WDF_TIMER                       EvtTimerFunc;

typedef struct _RPDO_DEVICE_DATA
{
	WDFQUEUE GetFeatureQueue;
	WDFQUEUE SetFeatureQueue;
	WDFQUEUE ReadReportQueue;
	WDFQUEUE WriteReportQueue;
	WDFQUEUE GetInputQueue;
	WDFQUEUE SetOutputQueue;
	BYTE	 TestBuffer[256];
} RPDO_DEVICE_DATA, *PRPDO_DEVICE_DATA;

WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(RPDO_DEVICE_DATA, GetRawDeviceContext);

typedef struct _DEVICE_CONTEXT
{
	WDFDEVICE               Device;
	PRPDO_DEVICE_DATA       RawDeviceContext;
	WDFQUEUE                DefaultQueue;
	HID_DEVICE_ATTRIBUTES   HidDeviceAttributes;
	HID_DESCRIPTOR          HidDescriptor;
	PHID_REPORT_DESCRIPTOR  ReportDescriptor;
	BOOLEAN                 ReadReportDescFromRegistry;
} DEVICE_CONTEXT, *PDEVICE_CONTEXT;

WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(DEVICE_CONTEXT, GetDeviceContext);


typedef struct _MANUAL_QUEUE_CONTEXT
{
	WDFREQUEST              Request;
	WDFTIMER                Timer;
} MANUAL_QUEUE_CONTEXT, *PMANUAL_QUEUE_CONTEXT;

WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(MANUAL_QUEUE_CONTEXT, GetManualQueueContext);

NTSTATUS
VirtualHID_QueueCreate(
	_In_  WDFDEVICE         Device,
	_Out_ WDFQUEUE          *Queue
);

VOID
VirtualHID_EvtIoDeviceControl(
	_In_  WDFQUEUE          Queue,
	_In_  WDFREQUEST        Request,
	_In_  size_t            OutputBufferLength,
	_In_  size_t            InputBufferLength,
	_In_  ULONG             IoControlCode
);

NTSTATUS
VirtualHID_ForwardRequest(
	_In_  WDFQUEUE    Queue,
	_In_  WDFREQUEST        Request,
	_Always_(_Out_)
	BOOLEAN*          CompleteRequest,
	_In_ char * ioctlName

);

NTSTATUS
VirtualHID_GetString(
	_In_  WDFREQUEST        Request
);

NTSTATUS
VirtualHID_GetIndexedString(
	_In_  WDFREQUEST        Request
);

NTSTATUS
VirtualHID_GetStringId(
	_In_  WDFREQUEST        Request,
	_Out_ ULONG            *StringId,
	_Out_ ULONG            *LanguageId
);

NTSTATUS
VirtualHID_RequestCopyFromBuffer(
	_In_  WDFREQUEST        Request,
	_In_  PVOID             SourceBuffer,
	_When_(NumBytesToCopyFrom == 0, __drv_reportError(NumBytesToCopyFrom cannot be zero))
	_In_  size_t            NumBytesToCopyFrom
);

NTSTATUS
VirtualHID_RequestCopyToBuffer(
	_In_  WDFREQUEST        Request,
	_In_  PVOID             DestinationBuffer,
	_When_(NumBytesToCopyTo == 0, __drv_reportError(NumBytesToCopyTo cannot be zero))
	_In_  size_t            NumBytesToCopyTo
);

NTSTATUS
RequestGetHidXferPacket_ToReadFromDevice(
	_In_  WDFREQUEST        Request,
	_Out_ HID_XFER_PACKET  *Packet
);

NTSTATUS
RequestGetHidXferPacket_ToWriteToDevice(
	_In_  WDFREQUEST        Request,
	_Out_ HID_XFER_PACKET  *Packet
);

NTSTATUS
VirtualHID_CreateRawPdo(
	WDFDEVICE       Device
);
//
// Misc definitions
//
#define CONTROL_FEATURE_REPORT_ID   0x01

//
// These are the device attributes returned by the mini driver in response
// to IOCTL_HID_GET_DEVICE_ATTRIBUTES.
//
#define HIDMINI_PID             0xD335
#define HIDMINI_VID             0xD335
#define HIDMINI_VERSION         0x0101

#define DebugPrint(_x_) DbgPrint _x_

// Function to initialize the device and its callbacks
//
NTSTATUS
VirtualHID_CreateDevice(
	_Inout_ PWDFDEVICE_INIT DeviceInit
);


EXTERN_C_END
