/*--

Copyright (c) Microsoft Corporation.  All rights reserved.

	THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
	KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
	IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
	PURPOSE.

Module Name:

	RawPdo.c

Abstract: This module have the code enumerate a raw PDO for every device
		  the filter attaches to so that it can provide a direct
		  sideband communication with the usermode application.

		  The toaster filter driver sample demonstrates an alternation
		  approach where you can create one control-device for all the
		  instances of the filter device.

Environment:

	Kernel mode only.

--*/

#include "driver.h"

#define READ_REPORT_MIN_SIZE 3

NTSTATUS CompleteReadRequest(
	WDFREQUEST* Request,
	WDFQUEUE*	Queue
);

NTSTATUS ProcessReadRequest(
	WDFREQUEST* Request,
	WDFQUEUE*	Queue,
	WDFQUEUE*	CompleteQueue
);

NTSTATUS ManualQueueCreate(
	_In_  WDFDEVICE         Device,
	_Out_ WDFQUEUE          *Queue
);

NTSTATUS CompleteGetRequest(
	WDFREQUEST* Request,
	WDFQUEUE* Queue
);

NTSTATUS ProcessGetRequest(
	WDFREQUEST* Request,
	WDFQUEUE* Queue,
	WDFQUEUE* CompleteQueue

);

NTSTATUS ProcessSetRequest(
	WDFREQUEST* Request,
	WDFQUEUE* Queue
);

VOID
VirtualHID_EvtIoDeviceControlForRawPdo(
	IN WDFQUEUE      Queue,
	IN WDFREQUEST    Request,
	IN size_t        OutputBufferLength,
	IN size_t        InputBufferLength,
	IN ULONG         IoControlCode
)
/*++

Routine Description:

	This routine is the dispatch routine for device control requests.

Arguments:

	Queue - Handle to the framework queue object that is associated
			with the I/O request.
	Request - Handle to a framework request object.

	OutputBufferLength - length of the request's output buffer,
						if an output buffer is available.
	InputBufferLength - length of the request's input buffer,
						if an input buffer is available.

	IoControlCode - the driver-defined or system-defined I/O control code
					(IOCTL) that is associated with the request.

Return Value:

   VOID

--*/
{
	NTSTATUS status = STATUS_SUCCESS;
	WDFDEVICE parent = WdfIoQueueGetDevice(Queue);
	PRPDO_DEVICE_DATA pdoData;

	UNREFERENCED_PARAMETER(OutputBufferLength);
	UNREFERENCED_PARAMETER(InputBufferLength);

	pdoData = GetRawDeviceContext(parent);

	KdPrint(("Entered KbFilter_EvtIoDeviceControlForRawPdo\n"));

	//
	// Process the ioctl and complete it when you are done.
	// Since the queue is configured for serial dispatch, you will
	// not receive another ioctl request until you complete this one.
	//

	switch (IoControlCode) {
	case SET_FEATURE:
		status = ProcessSetRequest(&Request, &pdoData->SetFeatureQueue);
		break;
	case GET_FEATURE:
		status = ProcessGetRequest(&Request, &pdoData->GetFeatureQueue, &pdoData->CompleteGetFeatureQueue);
		break;
	case COMPLETE_GET_FEATURE:
		status = CompleteGetRequest(&Request, &pdoData->CompleteGetFeatureQueue);
		break;
	case READ_REPORT:
		KdPrint(("read report\n"));
		status = ProcessReadRequest(&Request, &pdoData->ReadReportQueue, &pdoData->CompleteReadReportQueue);
		break;
	case COMPLETE_READ_REPORT:
		KdPrint(("complete read report\n"));
		status = CompleteReadRequest(&Request, &pdoData->CompleteReadReportQueue);
		break;
	case WRITE_REPORT:
		status = ProcessSetRequest(&Request, &pdoData->WriteReportQueue);
		break;
	case GET_INPUT_REPORT:
		status = ProcessGetRequest(&Request, &pdoData->GetInputQueue, &pdoData->CompleteGetInputQueue);
		break;
	case COMPLETE_GET_INPUT_REPORT:
		status = CompleteGetRequest(&Request, &pdoData->CompleteGetInputQueue);
		break;
	case SET_OUTPUT_REPORT:
		status = ProcessSetRequest(&Request, &pdoData->SetOutputQueue);
		break;
	case TEST_WRITE:
		KdPrint(("test_write\n"));
		status = VirtualHID_RequestCopyToBuffer(Request, &pdoData->TestBuffer, sizeof(pdoData->TestBuffer));
		KdPrint(("test_write status %x\n", status));
		break;
	case TEST_READ:
		KdPrint(("test_read\n"));
		status = VirtualHID_RequestCopyFromBuffer(Request, &pdoData->TestBuffer, sizeof(pdoData->TestBuffer));
		KdPrint(("test_read status %x\n", status));
		break;
	default:
		status = STATUS_NOT_IMPLEMENTED;
		break;
	}

	WdfRequestComplete(Request, status);
	return;
}

NTSTATUS CompleteReadRequest(
	WDFREQUEST* Request,
	WDFQUEUE* Queue
)
{
	WDFREQUEST				request;
	NTSTATUS				status = STATUS_SUCCESS;
	PVOID					inBuffer;
	size_t	                bytesDelivered = 0;

	status = WdfIoQueueRetrieveNextRequest(*Queue, &request);
	if (!NT_SUCCESS(status))
	{
		KdPrint(("WdfIoQueueRetrieveNextRequest failed: %x", status));
		return status;
	}
	status = WdfRequestRetrieveInputBuffer(*Request, READ_REPORT_MIN_SIZE, &inBuffer, &bytesDelivered);

	if (!NT_SUCCESS(status))
	{
		KdPrint(("WdfRequestRetrieveInputBuffer failed: %x", status));
		WdfRequestComplete(request, STATUS_CANCELLED);
		return status;
	}

	if (bytesDelivered < READ_REPORT_MIN_SIZE)
	{
		WdfRequestComplete(request, STATUS_CANCELLED);
		return STATUS_BUFFER_TOO_SMALL;
	}

	status = VirtualHID_RequestCopyFromBuffer(request, inBuffer, bytesDelivered);

	WdfRequestComplete(request, status);
	WdfRequestSetInformation(*Request, bytesDelivered);

	return status;
}

NTSTATUS ProcessReadRequest(
	WDFREQUEST* Request,
	WDFQUEUE*	Queue,
	WDFQUEUE*	CompleteQueue
)
{
	WDFREQUEST				request;
	NTSTATUS				status = STATUS_SUCCESS;

	status = WdfIoQueueRetrieveNextRequest(
		*Queue,
		&request);

	if (!NT_SUCCESS(status))
	{
		return status;
	}

	status = WdfRequestForwardToIoQueue(request, *CompleteQueue);

	if (!NT_SUCCESS(status))
	{
		return status;
	}

	WdfRequestSetInformation(*Request, 0);

	return status;
}

NTSTATUS ProcessSetRequest(
	WDFREQUEST* Request,
	WDFQUEUE*	Queue
)
{
	WDFREQUEST				request;
	HID_XFER_PACKET         packet;
	NTSTATUS				status;

	status = WdfIoQueueRetrieveNextRequest(
		*Queue,
		&request);

	if (!NT_SUCCESS(status))
	{
		return status;
	}

	status = RequestGetHidXferPacket_ToWriteToDevice(
		request,
		&packet);

	if (!NT_SUCCESS(status))
	{
		WdfRequestComplete(request, status);
		return status;
	}

	status = VirtualHID_RequestCopyFromBuffer(*Request, packet.reportBuffer, packet.reportBufferLen);
	if (NT_SUCCESS(status))
	{
		WdfRequestCompleteWithInformation(request, status, packet.reportBufferLen);
	}
	else
	{
		WdfRequestComplete(request, status);
	}

	return status;
}

NTSTATUS ProcessGetRequest(
	WDFREQUEST* Request,
	WDFQUEUE*	Queue,
	WDFQUEUE*	CompleteQueue
)
{
	WDFREQUEST				request;
	HID_XFER_PACKET         packet;
	NTSTATUS				status;

	status = WdfIoQueueRetrieveNextRequest(
		*Queue,
		&request);

	if (!NT_SUCCESS(status))
	{
		return status;
	}

	status = RequestGetHidXferPacket_ToReadFromDevice(request, &packet);

	if (!NT_SUCCESS(status))
	{
		return status;
	}

	status = VirtualHID_RequestCopyFromBuffer(*Request, packet.reportBuffer, packet.reportBufferLen);

	if (!NT_SUCCESS(status))
	{
		return status;
	}

	status = WdfRequestForwardToIoQueue(request, *CompleteQueue);

	return status;
}

NTSTATUS CompleteGetRequest(
	WDFREQUEST* Request,
	WDFQUEUE* Queue
)
{
	WDFREQUEST request;
	HID_XFER_PACKET         packet;
	NTSTATUS status = STATUS_SUCCESS;

	status = WdfIoQueueRetrieveNextRequest(
		*Queue,
		&request);

	if (!NT_SUCCESS(status))
	{
		return status;
	}

	status = RequestGetHidXferPacket_ToReadFromDevice(
		request,
		&packet);
	if (!NT_SUCCESS(status)) {
		return status;
	}

	status = VirtualHID_RequestCopyToBuffer(*Request, &packet.reportBuffer, packet.reportBufferLen);
	if (!NT_SUCCESS(status))
	{
		WdfRequestComplete(request, STATUS_CANCELLED);
		return status;
	}

	WdfRequestCompleteWithInformation(request, status, packet.reportBufferLen);

	return status;
}
#define MAX_ID_LEN 128

NTSTATUS
VirtualHID_CreateRawPdo(
	WDFDEVICE       Device
)
/*++

Routine Description:

	This routine creates and initialize a PDO.

Arguments:

Return Value:

	NT Status code.

--*/
{
	NTSTATUS                    status;
	PWDFDEVICE_INIT             pDeviceInit = NULL;
	PRPDO_DEVICE_DATA           pdoData = NULL;
	WDFDEVICE                   hChild = NULL;
	WDF_OBJECT_ATTRIBUTES       pdoAttributes;
	WDF_DEVICE_PNP_CAPABILITIES pnpCaps;
	WDF_IO_QUEUE_CONFIG         ioQueueConfig;
	WDFQUEUE                    queue;
	WDF_DEVICE_STATE            deviceState;
	PDEVICE_CONTEXT	            devExt;
	DECLARE_CONST_UNICODE_STRING(deviceId, VHID_RAW_DEVICE_ID);
	DECLARE_CONST_UNICODE_STRING(hardwareId, VHID_HARDWARE_ID);
	DECLARE_CONST_UNICODE_STRING(deviceLocation, L"Virtual Hid\0");
	DECLARE_UNICODE_STRING_SIZE(buffer, MAX_ID_LEN);

	KdPrint(("Entered VirtualHID_CreateRawPdo\n"));

	//
	// Allocate a WDFDEVICE_INIT structure and set the properties
	// so that we can create a device object for the child.
	//
	pDeviceInit = WdfPdoInitAllocate(Device);

	if (pDeviceInit == NULL) {
		status = STATUS_INSUFFICIENT_RESOURCES;
		goto Cleanup;
	}
	WdfDeviceInitSetExclusive(pDeviceInit, TRUE);

	//
	// Mark the device RAW so that the child device can be started
	// and accessed without requiring a function driver. Since we are
	// creating a RAW PDO, we must provide a class guid.
	//
	status = WdfPdoInitAssignRawDevice(pDeviceInit, &GUID_DEVCLASS_UNKNOWN);
	if (!NT_SUCCESS(status)) {
		goto Cleanup;
	}

	//
	// Since keyboard is secure device, we must protect ourselves from random
	// users sending ioctls and creating trouble.
	//
	status = WdfDeviceInitAssignSDDLString(pDeviceInit,
		&SDDL_DEVOBJ_SYS_ALL_ADM_RWX_WORLD_RWX_RES_RWX);
	if (!NT_SUCCESS(status)) {
		goto Cleanup;
	}

	//
	// Assign DeviceID - This will be reported to IRP_MN_QUERY_ID/BusQueryDeviceID
	//
	status = WdfPdoInitAssignDeviceID(pDeviceInit, &deviceId);
	if (!NT_SUCCESS(status)) {
		goto Cleanup;
	}

	//
	// For RAW PDO, there is no need to provide BusQueryHardwareIDs
	// and BusQueryCompatibleIDs IDs unless we are running on
	// Windows 2000.
	//
	if (!RtlIsNtDdiVersionAvailable(NTDDI_WINXP)) {
		//
		// On Win2K, we must provide a HWID for the device to get enumerated.
		// Since we are providing a HWID, we will have to provide a NULL inf
		// to avoid the "found new device" popup and get the device installed
		// silently.
		//
		status = WdfPdoInitAddHardwareID(pDeviceInit, &hardwareId);
		if (!NT_SUCCESS(status)) {
			goto Cleanup;
		}
	}

	//
	// We could be enumerating more than one children if the filter attaches
	// to multiple instances of keyboard, so we must provide a
	// BusQueryInstanceID. If we don't, system will throw CA bugcheck.
	//
	status = RtlUnicodeStringPrintf(&buffer, L"%02d", 0);
	if (!NT_SUCCESS(status)) {
		goto Cleanup;
	}

	status = WdfPdoInitAssignInstanceID(pDeviceInit, &buffer);
	if (!NT_SUCCESS(status)) {
		goto Cleanup;
	}

	//
	// Provide a description about the device. This text is usually read from
	// the device. In the case of USB device, this text comes from the string
	// descriptor. This text is displayed momentarily by the PnP manager while
	// it's looking for a matching INF. If it finds one, it uses the Device
	// Description from the INF file to display in the device manager.
	// Since our device is raw device and we don't provide any hardware ID
	// to match with an INF, this text will be displayed in the device manager.
	//
	status = RtlUnicodeStringPrintf(&buffer, L"Virtual_HID_%02d", 0);
	if (!NT_SUCCESS(status)) {
		goto Cleanup;
	}

	//
	// You can call WdfPdoInitAddDeviceText multiple times, adding device
	// text for multiple locales. When the system displays the text, it
	// chooses the text that matches the current locale, if available.
	// Otherwise it will use the string for the default locale.
	// The driver can specify the driver's default locale by calling
	// WdfPdoInitSetDefaultLocale.
	//
	status = WdfPdoInitAddDeviceText(pDeviceInit,
		&buffer,
		&deviceLocation,
		0x409
	);
	if (!NT_SUCCESS(status)) {
		goto Cleanup;
	}

	WdfPdoInitSetDefaultLocale(pDeviceInit, 0x409);

	//
	// Initialize the attributes to specify the size of PDO device extension.
	// All the state information private to the PDO will be tracked here.
	//
	WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(&pdoAttributes, RPDO_DEVICE_DATA);

	//
	// Set up our queue to allow forwarding of requests to the parent
	// This is done so that the cached Keyboard Attributes can be retrieved
	//
	//WdfPdoInitAllowForwardingRequestToParent(pDeviceInit);

	status = WdfDeviceCreate(&pDeviceInit, &pdoAttributes, &hChild);
	if (!NT_SUCCESS(status)) {
		goto Cleanup;
	}

	//
	// Get the device context.
	//
	devExt = GetDeviceContext(Device);

	pdoData = GetRawDeviceContext(hChild);
	devExt->RawDeviceContext = pdoData;

	// Configure the default queue associated with the control device object
	// to be Serial so that request passed to EvtIoDeviceControl are serialized.
	// A default queue gets all the requests that are not
	// configure-fowarded using WdfDeviceConfigureRequestDispatching.
	//

	WDF_IO_QUEUE_CONFIG_INIT_DEFAULT_QUEUE(&ioQueueConfig,
		WdfIoQueueDispatchSequential);

	ioQueueConfig.EvtIoDeviceControl = VirtualHID_EvtIoDeviceControlForRawPdo;

	status = WdfIoQueueCreate(hChild,
		&ioQueueConfig,
		WDF_NO_OBJECT_ATTRIBUTES,
		&queue // pointer to default queue
	);
	if (!NT_SUCCESS(status)) {
		KdPrint(("WdfEventRawIoQueueCreate failed 0x%x\n", status));
		goto Cleanup;
	}

	//
	// Set some properties for the child device.
	//
	WDF_DEVICE_PNP_CAPABILITIES_INIT(&pnpCaps);

	pnpCaps.Removable = WdfTrue;
	pnpCaps.SurpriseRemovalOK = WdfTrue;
	pnpCaps.NoDisplayInUI = WdfTrue;

	pnpCaps.Address = 0;
	pnpCaps.UINumber = 0;

	WdfDeviceSetPnpCapabilities(hChild, &pnpCaps);

	//
	// TODO: In addition to setting NoDisplayInUI in DeviceCaps, we
	// have to do the following to hide the device. Following call
	// tells the framework to report the device state in
	// IRP_MN_QUERY_DEVICE_STATE request.
	//
	WDF_DEVICE_STATE_INIT(&deviceState);
	deviceState.DontDisplayInUI = WdfTrue;
	WdfDeviceSetDeviceState(hChild, &deviceState);

	//
	// Tell the Framework that this device will need an interface so that
	// application can find our device and talk to it.
	//
	status = WdfDeviceCreateDeviceInterface(
		hChild,
		&GUID_DEVINTERFACE_VIRTUALHID,
		NULL
	);
	if (!NT_SUCCESS(status)) {
		KdPrint(("WdfDeviceCreateDeviceInterface failed 0x%x\n", status));
		goto Cleanup;
	}

	status = ManualQueueCreate(Device, &pdoData->GetFeatureQueue);
	status = ManualQueueCreate(Device, &pdoData->SetFeatureQueue);
	status = ManualQueueCreate(Device, &pdoData->ReadReportQueue);
	status = ManualQueueCreate(Device, &pdoData->WriteReportQueue);
	status = ManualQueueCreate(Device, &pdoData->GetInputQueue);
	status = ManualQueueCreate(Device, &pdoData->SetOutputQueue);
	status = ManualQueueCreate(Device, &pdoData->CompleteGetFeatureQueue);
	status = ManualQueueCreate(Device, &pdoData->CompleteGetInputQueue);
	status = ManualQueueCreate(Device, &pdoData->CompleteReadReportQueue);

	if (!NT_SUCCESS(status)) {
		goto Cleanup;
	}

	//
	// Add this device to the FDO's collection of children.
	// After the child device is added to the static collection successfully,
	// driver must call WdfPdoMarkMissing to get the device deleted. It
	// shouldn't delete the child device directly by calling WdfObjectDelete.
	//
	status = WdfFdoAddStaticChild(Device, hChild);
	if (!NT_SUCCESS(status)) {
		KdPrint(("WdfFdoAddStaticChild failed %#x", status));
		goto Cleanup;
	}

	//
	// pDeviceInit will be freed by WDF.
	//
	return STATUS_SUCCESS;

Cleanup:

	KdPrint(("VirtualHID_CreatePdo failed %x\n", status));

	//
	// Call WdfDeviceInitFree if you encounter an error while initializing
	// a new framework device object. If you call WdfDeviceInitFree,
	// do not call WdfDeviceCreate.
	//
	if (pDeviceInit != NULL) {
		WdfDeviceInitFree(pDeviceInit);
	}

	if (hChild) {
		WdfObjectDelete(hChild);
	}

	return status;
}

NTSTATUS
ManualQueueCreate(
	_In_  WDFDEVICE         Device,
	_Out_ WDFQUEUE          *Queue
)
/*++
Routine Description:

This function creates a manual I/O queue to receive IOCTL_HID_READ_REPORT
forwarded from the device's default queue handler.

It also creates a periodic timer to check the queue and complete any pending
request with data from the device. Here timer expiring is used to simulate
a hardware event that new data is ready.

The workflow is like this:

- Hidclass.sys sends an ioctl to the miniport to read input report.

- The request reaches the driver's default queue. As data may not be avaiable
yet, the request is forwarded to a second manual queue temporarily.

- Later when data is ready (as simulated by timer expiring), the driver
checks for any pending request in the manual queue, and then completes it.

- Hidclass gets notified for the read request completion and return data to
the caller.

On the other hand, for IOCTL_HID_WRITE_REPORT request, the driver simply
sends the request to the hardware (as simulated by storing the data at
DeviceContext->DeviceData) and completes the request immediately. There is
no need to use another queue for write operation.

Arguments:

Device - Handle to a framework device object.

Queue - Output pointer to a framework I/O queue handle, on success.

Return Value:

NTSTATUS

--*/
{
	NTSTATUS                status;
	WDF_IO_QUEUE_CONFIG     queueConfig;
	WDFQUEUE                queue;

	WDF_IO_QUEUE_CONFIG_INIT(
		&queueConfig,
		WdfIoQueueDispatchManual);

	status = WdfIoQueueCreate(
		Device,
		&queueConfig,
		WDF_NO_OBJECT_ATTRIBUTES,
		&queue);

	if (!NT_SUCCESS(status)) {
		KdPrint(("WdfManualIoQueueCreate failed 0x%x\n", status));
		return status;
	}

	*Queue = queue;

	return status;
}