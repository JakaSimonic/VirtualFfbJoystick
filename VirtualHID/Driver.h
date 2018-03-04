/*++

Module Name:

	driver.h

Abstract:

	This file contains the driver definitions.

Environment:

	Kernel-mode Driver Framework

--*/

#include <ntddk.h>
#include <wdf.h>
#include <hidport.h>  // located in $(DDK_INC_PATH)/wdm
#include <ntdef.h>
#define NTSTRSAFE_LIB
#include <ntstrsafe.h>

#include <initguid.h>
#include <devguid.h>

#include "common.h"
#include "device.h"
#include "trace.h"
#include "public.h"

EXTERN_C_START

//
// WDFDRIVER Events
//

DRIVER_INITIALIZE DriverEntry;
EVT_WDF_DRIVER_DEVICE_ADD VirtualHIDEvtDeviceAdd;
EVT_WDF_OBJECT_CONTEXT_CLEANUP VirtualHIDEvtDriverContextCleanup;

EXTERN_C_END

/*++

Copyright (C) Microsoft Corporation, All Rights Reserved

Module Name:

vhidmini.h

Abstract:

This module contains the type definitions for the driver

Environment:

Windows Driver Framework (WDF)

--*/
