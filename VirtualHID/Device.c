/*++

Module Name:

	device.c - Device handling events for example driver.

Abstract:

   This file contains the device entry points and callbacks.

Environment:

	Kernel-mode Driver Framework

--*/

#include "driver.h"
#include "device.tmh"

#ifdef ALLOC_PRAGMA
#pragma alloc_text (PAGE, VirtualHID_CreateDevice)
#endif

#pragma region Descriptors
HID_REPORT_DESCRIPTOR       G_DefaultReportDescriptor[] = {
	0x05, 0x01,                    // USAGE_PAGE (Generic Desktop)
	0x09, 0x04,                    // USAGE (Joystick)
	0xa1, 0x01,                    // COLLECTION (Application)
	0x85, 0x01, 				   // REPORT_ID (1)
	0x16, 0x01, 0x80,              //   LOGICAL_MINIMUM (-32767)
	0x26, 0xff, 0x7f,              //   LOGICAL_MAXIMUM (32767)
	0x75, 0x10,                    //   REPORT_SIZE (16)
	0x95, 0x08,                    //   REPORT_COUNT (8)
	/***************** Eight Axes ************************************/
	0x09, 0x01,                    //   USAGE (Pointer)
	0xa1, 0x00,                    //   COLLECTION (Physical)
	0x09, 0x30,                    //     USAGE (X)
	0x09, 0x31,                    //     USAGE (Y)
	0x09, 0x32,                    //     USAGE (Z)
	0x09, 0x33,                    //     USAGE (Rx)
	0x09, 0x34,                    //     USAGE (Ry)
	0x09, 0x35,                    //     USAGE (Rz)
	0x09, 0x36,                    //     USAGE (Slider)
	0x09, 0x37,                    //     USAGE (Dial)
	0x81, 0x02,                    //     INPUT (Data,Var,Abs)
	0xc0,                          //   END_COLLECTION
	/***************** 128 Buttons ************************************/
	0x05, 0x09,                    //   USAGE_PAGE (Button)
	0x19, 0x01,                    //   USAGE_MINIMUM (Button 1)
	0x29, 0xF0,                    //   USAGE_MAXIMUM (Button 128)
	0x15, 0x00,                    //   LOGICAL_MINIMUM (0)
	0x25, 0x01,                    //   LOGICAL_MAXIMUM (1)
	0x75, 0x01,                    //   REPORT_SIZE (1)
	0x95, 0xF0,                    //   REPORT_COUNT (128)
	0x81, 0x02,                    //   INPUT (Data,Var,Abs)
	0x81, 0x01,                    // INPUT (Cnst,Ary,Abs)

	/************************FFB**************************************/
	0x05,0x0F,	// USAGE_PAGE (Physical Interface)
	0x09,0x92,	// USAGE (PID State Report)
	0xA1,0x02,	// COLLECTION (Logical)
	0x85,0x02,	// REPORT_ID (02)
	0x09,0x9F,	// USAGE (Device Paused)
	0x09,0xA0,	// USAGE (Actuators Enabled)
	0x09,0xA4,	// USAGE (Safety Switch)
	0x09,0xA5,	// USAGE (Actuator Override Switch)
	0x09,0xA6,	// USAGE (Actuator Power)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x25,0x01,	// LOGICAL_MINIMUM (01)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x45,0x01,	// PHYSICAL_MAXIMUM (01)
	0x75,0x01,	// REPORT_SIZE (01)
	0x95,0x05,	// REPORT_COUNT (05)
	0x81,0x02,	// INPUT (Data,Var,Abs)
	0x95,0x03,	// REPORT_COUNT (03)
	0x81,0x03,	// INPUT (Constant,Var,Abs)
	0x09,0x94,	// USAGE (Effect Playing)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x25,0x01,	// LOGICAL_MAXIMUM (01)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x45,0x01,	// PHYSICAL_MAXIMUM (01)
	0x75,0x01,	// REPORT_SIZE (01)
	0x95,0x01,	// REPORT_COUNT (01)
	0x81,0x02,	// INPUT (Data,Var,Abs)
	0x09,0x22,	// USAGE (Effect Block Index)
	0x15,0x01,	// LOGICAL_MINIMUM (01)
	0x25,0x28,	// LOGICAL_MAXIMUM (28)
	0x35,0x01,	// PHYSICAL_MINIMUM (01)
	0x45,0x28,	// PHYSICAL_MAXIMUM (28)
	0x75,0x07,	// REPORT_SIZE (07)
	0x95,0x01,	// REPORT_COUNT (01)
	0x81,0x02,	// INPUT (Data,Var,Abs)
	0xC0,	// END COLLECTION ()

	0x09,0x21,	// USAGE (Set Effect Report)
	0xA1,0x02,	// COLLECTION (Logical)
	0x85,0x01,	// REPORT_ID (01)
	0x09,0x22,	// USAGE (Effect Block Index)
	0x15,0x01,	// LOGICAL_MINIMUM (01)
	0x25,0x28,	// LOGICAL_MAXIMUM (28)
	0x35,0x01,	// PHYSICAL_MINIMUM (01)
	0x45,0x28,	// PHYSICAL_MAXIMUM (28)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x09,0x25,	// USAGE (25)
	0xA1,0x02,	// COLLECTION (Logical)
	0x09,0x26,	// USAGE (Constant Force)
	0x09,0x27,	// USAGE (Ramp)
	0x09,0x30,	// USAGE (Square)
	0x09,0x31,	// USAGE (Sine)
	0x09,0x32,	// USAGE (Triangle)
	0x09,0x33,	// USAGE (Sawtooth Up)
	0x09,0x34,	// USAGE (Sawtooth Down)
	0x09,0x40,	// USAGE (Spring)
	0x09,0x41,	// USAGE (Damper)
	0x09,0x42,	// USAGE (Inertia)
	0x09,0x43,	// USAGE (Friction)
	0x09,0x28,	// USAGE (Custom)
	0x25,0x0C,	// LOGICAL_MAXIMUM (0C)
	0x15,0x01,	// LOGICAL_MINIMUM (01)
	0x35,0x01,	// PHYSICAL_MINIMUM (01)
	0x45,0x0C,	// PHYSICAL_MAXIMUM (0C)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x00,	// OUTPUT (Data)
	0xC0,	// END COLLECTION ()
	0x09,0x50,	// USAGE (Duration)
	0x09,0x54,	// USAGE (Trigger Repeat Interval)
	0x09,0x51,	// USAGE (Sample Period)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x26,0xFF,0x7F,	// LOGICAL_MAXIMUM (7F FF)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x46,0xFF,0x7F,	// PHYSICAL_MAXIMUM (7F FF)
	0x66,0x03,0x10,	// UNIT (Eng Lin:Time)
	0x55,0xFD,	// UNIT_EXPONENT (-3)
	0x75,0x10,	// REPORT_SIZE (10)
	0x95,0x03,	// REPORT_COUNT (03)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x55,0x00,	// UNIT_EXPONENT (00)
	0x66,0x00,0x00,	// UNIT (None)
	0x09,0x52,	// USAGE (Gain)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x26,0xFF,0x00,	// LOGICAL_MAXIMUM (00 FF)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x46,0x10,0x27,	// PHYSICAL_MAXIMUM (10000)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x09,0x53,	// USAGE (Trigger Button)
	0x15,0x01,	// LOGICAL_MINIMUM (01)
	0x25,0x81,	// LOGICAL_MAXIMUM (129)
	0x35,0x01,	// PHYSICAL_MINIMUM (01)
	0x45,0x81,	// PHYSICAL_MAXIMUM (129)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x09,0x55,	// USAGE (Axes Enable)
	0xA1,0x02,	// COLLECTION (Logical)
	0x05,0x01,	// USAGE_PAGE (Generic Desktop)
	0x09,0x30,	// USAGE (X)
	0x09,0x31,	// USAGE (Y)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x25,0x01,	// LOGICAL_MAXIMUM (01)
	0x75,0x01,	// REPORT_SIZE (01)
	0x95,0x02,	// REPORT_COUNT (02)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0xC0,	// END COLLECTION ()
	0x05,0x0F,	// USAGE_PAGE (Physical Interface)
	0x09,0x56,	// USAGE (Direction Enable)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x95,0x05,	// REPORT_COUNT (05)
	0x91,0x03,	// OUTPUT (Constant,Var,Abs)
	0x09,0x57,	// USAGE (Direction)
	0xA1,0x02,	// COLLECTION (Logical)
	0x0B,0x01,0x00,0x0A,0x00,	//    Usage Ordinals: Instance 1
	0x0B,0x02,0x00,0x0A,0x00,	//    Usage Ordinals: Instance 2
	0x66,0x14,0x00,	// UNIT (Eng Rot:Angular Pos)
	0x55,0xFE,	// UNIT_EXPONENT (FE)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x26,0xFF,0x00,	// LOGICAL_MAXIMUM (00 FF)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x47,0xA0,0x8C,0x00,0x00,	// PHYSICAL_MAXIMUM (00 00 8C A0)
	0x66,0x00,0x00,	// UNIT (None)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x02,	// REPORT_COUNT (02)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x55,0x00,	// UNIT_EXPONENT (00)
	0x66,0x00,0x00,	// UNIT (None)
	0xC0,	// END COLLECTION ()
	0x05,0x0F,	// USAGE_PAGE (Physical Interface)
	0x09,0xA7,	// USAGE (Start Delay)
	0x66,0x03,0x10,	// UNIT (Eng Lin:Time)
	0x55,0xFD,	// UNIT_EXPONENT (-3)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x26,0xFF,0x7F,	// LOGICAL_MAXIMUM (7F FF)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x46,0xFF,0x7F,	// PHYSICAL_MAXIMUM (7F FF)
	0x75,0x10,	// REPORT_SIZE (10)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x66,0x00,0x00,	// UNIT (None)
	0x55,0x00,	// UNIT_EXPONENT (00)
	0xC0,	// END COLLECTION ()

	0x05,0x0F,	// USAGE_PAGE (Physical Interface)
	0x09,0x5A,	// USAGE (Set Envelope Report)
	0xA1,0x02,	// COLLECTION (Logical)
	0x85,0x02,	// REPORT_ID (02)
	0x09,0x22,	// USAGE (Effect Block Index)
	0x15,0x01,	// LOGICAL_MINIMUM (01)
	0x25,0x28,	// LOGICAL_MAXIMUM (28)
	0x35,0x01,	// PHYSICAL_MINIMUM (01)
	0x45,0x28,	// PHYSICAL_MAXIMUM (28)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x09,0x5B,	// USAGE (Attack Level)
	0x09,0x5D,	// USAGE (Fade Level)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x26,0xFF,0x00,	// LOGICAL_MAXIMUM (00 FF)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x46,0x10,0x27,	// PHYSICAL_MAXIMUM (10000)
	0x95,0x02,	// REPORT_COUNT (02)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x09,0x5C,	// USAGE (Attack time)
	0x09,0x5E,	// USAGE (Fade time)
	0x66,0x03,0x10,	// UNIT (Eng Lin:Time)
	0x55,0xFD,	// UNIT_EXPONENT (-3)
	0x26,0xFF,0x7F,	// LOGICAL_MAXIMUM (7F FF)
	0x46,0xFF,0x7F,	// PHYSICAL_MAXIMUM (7F FF)
	0x75,0x10,	// REPORT_SIZE (10)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x45,0x00,	// PHYSICAL_MAXIMUM (00)
	0x66,0x00,0x00,	// UNIT (None)
	0x55,0x00,	// UNIT_EXPONENT (00)
	0xC0,	// END COLLECTION ()

	0x09,0x5F,	// USAGE (Set Condition Report)
	0xA1,0x02,	// COLLECTION (Logical)
	0x85,0x03,	// REPORT_ID (03)
	0x09,0x22,	// USAGE (Effect Block Index)
	0x15,0x01,	// LOGICAL_MINIMUM (01)
	0x25,0x28,	// LOGICAL_MAXIMUM (28)
	0x35,0x01,	// PHYSICAL_MINIMUM (01)
	0x45,0x28,	// PHYSICAL_MAXIMUM (28)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x09,0x23,	// USAGE (Parameter Block Offset)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x25,0x01,	// LOGICAL_MAXIMUM (01)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x45,0x01,	// PHYSICAL_MAXIMUM (01)
	0x75,0x04,	// REPORT_SIZE (04)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x09,0x58,	// USAGE (Type Specific Block Offset)
	0xA1,0x02,	// COLLECTION (Logical)
	0x0B,0x01,0x00,0x0A,0x00,	// USAGE (Instance 1)
	0x0B,0x02,0x00,0x0A,0x00,	// USAGE (Instance 2)
	0x75,0x02,	// REPORT_SIZE (02)
	0x95,0x02,	// REPORT_COUNT (02)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0xC0,	// END COLLECTION ()
	0x16, 0xF0, 0xD8,	// LOGICAL_MINIMUM (-10000d)
	0x26, 0x10, 0x27,	// LOGICAL_MAXIMUM (10000d)
	0x36,0xF0,0xD8,	// PHYSICAL_MINIMUM (-10000)
	0x46,0x10,0x27,	// PHYSICAL_MAXIMUM (10000)
	0x09,0x60,	// USAGE (CP Offset)
	0x75,0x10,	// REPORT_SIZE (16)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x36,0xF0,0xD8,	// PHYSICAL_MINIMUM (-10000)
	0x46,0x10,0x27,	// PHYSICAL_MAXIMUM (10000)
	0x09,0x61,	// USAGE (Positive Coefficient)
	0x09,0x62,	// USAGE (Negative Coefficient)
	0x95,0x02,	// REPORT_COUNT (02)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x26,0xFF,0x00,	// LOGICAL_MAXIMUM (00 FF)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x46,0x10,0x27,	// PHYSICAL_MAXIMUM (10000)
	0x09,0x63,	// USAGE (Positive Saturation)
	0x09,0x64,	// USAGE (Negative Saturation)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x02,	// REPORT_COUNT (02)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x09,0x65,	// USAGE (Dead Band )
	0x46,0x10,0x27,	// PHYSICAL_MAXIMUM (10000)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0xC0,	// END COLLECTION ()

	0x09,0x6E,	// USAGE (Set Periodic Report)
	0xA1,0x02,	// COLLECTION (Logical)
	0x85,0x04,	// REPORT_ID (04)
	0x09,0x22,	// USAGE (Effect Block Index)
	0x15,0x01,	// LOGICAL_MINIMUM (01)
	0x25,0x28,	// LOGICAL_MAXIMUM (28)
	0x35,0x01,	// PHYSICAL_MINIMUM (01)
	0x45,0x28,	// PHYSICAL_MAXIMUM (28)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x09,0x70,	// USAGE (Magnitude)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x26,0xFF,0x00,	// LOGICAL_MAXIMUM (00 FF)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x46,0x10,0x27,	// PHYSICAL_MAXIMUM (10000)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x09,0x6F,	// USAGE (Offset)
	0x15,0x80,	// LOGICAL_MINIMUM (80)
	0x25,0x7F,	// LOGICAL_MAXIMUM (7F)
	0x36,0xF0,0xD8,	// PHYSICAL_MINIMUM (-10000)
	0x46,0x10,0x27,	// PHYSICAL_MAXIMUM (10000)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x09,0x71,	// USAGE (Phase)
	0x66,0x14,0x00,	// UNIT (Eng Rot:Angular Pos)
	0x55,0xFE,	// UNIT_EXPONENT (FE)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x26,0xFF,0x00,	// LOGICAL_MAXIMUM (00 FF)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x47,0xA0,0x8C,0x00,0x00,	// PHYSICAL_MAXIMUM (00 00 8C A0)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x09,0x72,	// USAGE (Period)
	0x26,0xFF,0x7F,	// LOGICAL_MAXIMUM (7F FF)
	0x46,0xFF,0x7F,	// PHYSICAL_MAXIMUM (7F FF)
	0x66,0x03,0x10,	// UNIT (Eng Lin:Time)
	0x55,0xFD,	// UNIT_EXPONENT (-3)
	0x75,0x10,	// REPORT_SIZE (10)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x66,0x00,0x00,	// UNIT (None)
	0x55,0x00,	// UNIT_EXPONENT (00)
	0xC0,	// END COLLECTION ()

	0x09,0x73,	// USAGE (Set Constant Force Report)
	0xA1,0x02,	// COLLECTION (Logical)
	0x85,0x05,	// REPORT_ID (05)
	0x09,0x22,	// USAGE (Effect Block Index)
	0x15,0x01,	// LOGICAL_MINIMUM (01)
	0x25,0x28,	// LOGICAL_MAXIMUM (28)
	0x35,0x01,	// PHYSICAL_MINIMUM (01)
	0x45,0x28,	// PHYSICAL_MAXIMUM (28)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x09,0x70,	// USAGE (Magnitude)
	0x16,0x01,0xFF,	// LOGICAL_MINIMUM (-255)
	0x26,0xFF,0x00,	// LOGICAL_MAXIMUM (255)
	0x36,0xF0,0xD8,	// PHYSICAL_MINIMUM (-10000)
	0x46,0x10,0x27,	// PHYSICAL_MAXIMUM (10000)
	0x75,0x10,	// REPORT_SIZE (10)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0xC0,	// END COLLECTION ()

	0x09,0x74,	// USAGE (Set Ramp Force Report)
	0xA1,0x02,	// COLLECTION (Logical)
	0x85,0x06,	// REPORT_ID (06)
	0x09,0x22,	// USAGE (Effect Block Index)
	0x15,0x01,	// LOGICAL_MINIMUM (01)
	0x25,0x28,	// LOGICAL_MAXIMUM (28)
	0x35,0x01,	// PHYSICAL_MINIMUM (01)
	0x45,0x28,	// PHYSICAL_MAXIMUM (28)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x09,0x75,	// USAGE (Ramp Start)
	0x09,0x76,	// USAGE (Ramp End)
	0x15,0x80,	// LOGICAL_MINIMUM (-128)
	0x25,0x7F,	// LOGICAL_MAXIMUM (127)
	0x36,0xF0,0xD8,	// PHYSICAL_MINIMUM (-10000)
	0x46,0x10,0x27,	// PHYSICAL_MAXIMUM (10000)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x02,	// REPORT_COUNT (02)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0xC0,	// END COLLECTION ()

	0x09,0x68,	// USAGE (Custom Force Data Report)
	0xA1,0x02,	// COLLECTION (Logical)
	0x85,0x07,	// REPORT_ID (07)
	0x09,0x22,	// USAGE (Effect Block Index)
	0x15,0x01,	// LOGICAL_MINIMUM (01)
	0x25,0x28,	// LOGICAL_MAXIMUM (28)
	0x35,0x01,	// PHYSICAL_MINIMUM (01)
	0x45,0x28,	// PHYSICAL_MAXIMUM (28)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x09,0x6C,	// USAGE (Custom Force Data Offset)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x26,0x10,0x27,	// LOGICAL_MAXIMUM (10000)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x46,0x10,0x27,	// PHYSICAL_MAXIMUM (10000)
	0x75,0x10,	// REPORT_SIZE (10)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x09,0x69,	// USAGE (Custom Force Data)
	0x15,0x81,	// LOGICAL_MINIMUM (-127)
	0x25,0x7F,	// LOGICAL_MAXIMUM (127)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x46,0xFF,0x00,	// PHYSICAL_MAXIMUM (255)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x0C,	// REPORT_COUNT (0C)
	0x92,0x02,0x01,	// OUTPUT ( Data,Var,Abs,Buf)
	0xC0,	// END COLLECTION ()

	0x09,0x66,	// USAGE (Download Force Sample)
	0xA1,0x02,	// COLLECTION (Logical)
	0x85,0x08,	// REPORT_ID (08)
	0x05,0x01,	// USAGE_PAGE (Generic Desktop)
	0x09,0x30,	// USAGE (X)
	0x09,0x31,	// USAGE (Y)
	0x15,0x81,	// LOGICAL_MINIMUM (-127)
	0x25,0x7F,	// LOGICAL_MAXIMUM (127)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x46,0xFF,0x00,	// PHYSICAL_MAXIMUM (255)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x02,	// REPORT_COUNT (02)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0xC0,	// END COLLECTION ()

	0x05,0x0F,	// USAGE_PAGE (Physical Interface)
	0x09,0x77,	// USAGE (Effect Operation Report)
	0xA1,0x02,	// COLLECTION (Logical)
	0x85,0x0A,	// REPORT_ID (0A)
	0x09,0x22,	// USAGE (Effect Block Index)
	0x15,0x01,	// LOGICAL_MINIMUM (01)
	0x25,0x28,	// LOGICAL_MAXIMUM (28)
	0x35,0x01,	// PHYSICAL_MINIMUM (01)
	0x45,0x28,	// PHYSICAL_MAXIMUM (28)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x09,0x78,	// USAGE (78)
	0xA1,0x02,	// COLLECTION (Logical)
	0x09,0x79,	// USAGE (Op Effect Start)
	0x09,0x7A,	// USAGE (Op Effect Start Solo)
	0x09,0x7B,	// USAGE (Op Effect Stop)
	0x15,0x01,	// LOGICAL_MINIMUM (01)
	0x25,0x03,	// LOGICAL_MAXIMUM (03)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x00,	// OUTPUT (Data,Ary,Abs)
	0xC0,	// END COLLECTION ()
	0x09,0x7C,	// USAGE (Loop Count)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x26,0xFF,0x00,	// LOGICAL_MAXIMUM (00 FF)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x46,0xFF,0x00,	// PHYSICAL_MAXIMUM (00 FF)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0xC0,	// END COLLECTION ()

	0x09,0x90,	// USAGE (PID Block Free Report)
	0xA1,0x02,	// COLLECTION (Logical)
	0x85,0x0B,	// REPORT_ID (0B)
	0x09,0x22,	// USAGE (Effect Block Index)
	0x25,0x28,	// LOGICAL_MAXIMUM (28)
	0x15,0x01,	// LOGICAL_MINIMUM (01)
	0x35,0x01,	// PHYSICAL_MINIMUM (01)
	0x45,0x28,	// PHYSICAL_MAXIMUM (28)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0xC0,	// END COLLECTION ()

	0x09,0x96,	// USAGE (PID Device Control)
	0xA1,0x02,	// COLLECTION (Logical)
	0x85,0x0C,	// REPORT_ID (0C)
	0x09,0x97,	// USAGE (DC Enable Actuators)
	0x09,0x98,	// USAGE (DC Disable Actuators)
	0x09,0x99,	// USAGE (DC Stop All Effects)
	0x09,0x9A,	// USAGE (DC Device Reset)
	0x09,0x9B,	// USAGE (DC Device Pause)
	0x09,0x9C,	// USAGE (DC Device Continue)
	0x15,0x01,	// LOGICAL_MINIMUM (01)
	0x25,0x06,	// LOGICAL_MAXIMUM (06)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x00,	// OUTPUT (Data)
	0xC0,	// END COLLECTION ()

	0x09,0x7D,	// USAGE (Device Gain Report)
	0xA1,0x02,	// COLLECTION (Logical)
	0x85,0x0D,	// REPORT_ID (0D)
	0x09,0x7E,	// USAGE (Device Gain)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x26,0xFF,0x00,	// LOGICAL_MAXIMUM (00 FF)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x46,0x10,0x27,	// PHYSICAL_MAXIMUM (10000)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0xC0,	// END COLLECTION ()

	0x09,0x6B,	// USAGE (Set Custom Force Report)
	0xA1,0x02,	// COLLECTION (Logical)
	0x85,0x0E,	// REPORT_ID (0E)
	0x09,0x22,	// USAGE (Effect Block Index)
	0x15,0x01,	// LOGICAL_MINIMUM (01)
	0x25,0x28,	// LOGICAL_MAXIMUM (28)
	0x35,0x01,	// PHYSICAL_MINIMUM (01)
	0x45,0x28,	// PHYSICAL_MAXIMUM (28)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x09,0x6D,	// USAGE (Sample Count)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x26,0xFF,0x00,	// LOGICAL_MAXIMUM (00 FF)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x46,0xFF,0x00,	// PHYSICAL_MAXIMUM (00 FF)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x09,0x51,	// USAGE (Sample Period)
	0x66,0x03,0x10,	// UNIT (Eng Lin:Time)
	0x55,0xFD,	// UNIT_EXPONENT (-3)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x26,0xFF,0x7F,	// LOGICAL_MAXIMUM (32767)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x46,0xFF,0x7F,	// PHYSICAL_MAXIMUM (32767)
	0x75,0x10,	// REPORT_SIZE (10)
	0x95,0x01,	// REPORT_COUNT (01)
	0x91,0x02,	// OUTPUT (Data,Var,Abs)
	0x55,0x00,	// UNIT_EXPONENT (00)
	0x66,0x00,0x00,	// UNIT (None)
	0xC0,	// END COLLECTION ()

	0x09,0xAB,	// USAGE (Create New Effect Report)
	0xA1,0x02,	// COLLECTION (Logical)
	0x85,0x05,	// REPORT_ID (05)
	0x09,0x25,	// USAGE (Effect Type)
	0xA1,0x02,	// COLLECTION (Logical)
	0x09,0x26,	// USAGE (26)
	0x09,0x27,	// USAGE (27)
	0x09,0x30,	// USAGE (30)
	0x09,0x31,	// USAGE (31)
	0x09,0x32,	// USAGE (32)
	0x09,0x33,	// USAGE (33)
	0x09,0x34,	// USAGE (34)
	0x09,0x40,	// USAGE (40)
	0x09,0x41,	// USAGE (41)
	0x09,0x42,	// USAGE (42)
	0x09,0x43,	// USAGE (43)
	0x09,0x28,	// USAGE (28)
	0x25,0x0C,	// LOGICAL_MAXIMUM (0C)
	0x15,0x01,	// LOGICAL_MINIMUM (01)
	0x35,0x01,	// PHYSICAL_MINIMUM (01)
	0x45,0x0C,	// PHYSICAL_MAXIMUM (0C)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0xB1,0x00,	// FEATURE (Data)
	0xC0,	// END COLLECTION ()
	0x05,0x01,	// USAGE_PAGE (Generic Desktop)
	0x09,0x3B,	// USAGE (Byte Count)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x26,0xFF,0x01,	// LOGICAL_MAXIMUM (511)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x46,0xFF,0x01,	// PHYSICAL_MAXIMUM (511)
	0x75,0x0A,	// REPORT_SIZE (0A)
	0x95,0x01,	// REPORT_COUNT (01)
	0xB1,0x02,	// FEATURE (Data,Var,Abs)
	0x75,0x06,	// REPORT_SIZE (06)
	0xB1,0x01,	// FEATURE (Constant,Ary,Abs)
	0xC0,	// END COLLECTION ()

	0x05,0x0F,	// USAGE_PAGE (Physical Interface)
	0x09,0x89,	// USAGE (PID Block Load Report)
	0xA1,0x02,	// COLLECTION (Logical)
	0x85,0x06,	// REPORT_ID (06)
	0x09,0x22,	// USAGE (Effect Block Index)
	0x25,0x28,	// LOGICAL_MAXIMUM (28)
	0x15,0x01,	// LOGICAL_MINIMUM (01)
	0x35,0x01,	// PHYSICAL_MINIMUM (01)
	0x45,0x28,	// PHYSICAL_MAXIMUM (28)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0xB1,0x02,	// FEATURE (Data,Var,Abs)
	0x09,0x8B,	// USAGE (Block Load Status)
	0xA1,0x02,	// COLLECTION (Logical)
	0x09,0x8C,	// USAGE (Block Load Success)
	0x09,0x8D,	// USAGE (Block Load Full)
	0x09,0x8E,	// USAGE (Block Load Error)
	0x25,0x03,	// LOGICAL_MAXIMUM (03)
	0x15,0x01,	// LOGICAL_MINIMUM (01)
	0x35,0x01,	// PHYSICAL_MINIMUM (01)
	0x45,0x03,	// PHYSICAL_MAXIMUM (03)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0xB1,0x00,	// FEATURE (Data)
	0xC0,	// END COLLECTION ()
	0x09,0xAC,	// USAGE (RAM Pool Available)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x27,0xFF,0xFF,0x00,0x00,	// LOGICAL_MAXIMUM (00 00 FF FF)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x47,0xFF,0xFF,0x00,0x00,	// PHYSICAL_MAXIMUM (00 00 FF FF)
	0x75,0x10,	// REPORT_SIZE (10)
	0x95,0x01,	// REPORT_COUNT (01)
	0xB1,0x00,	// FEATURE (Data)
	0xC0,	// END COLLECTION ()

	0x09,0x7F,	// USAGE (PID Pool Report)
	0xA1,0x02,	// COLLECTION (Logical)
	0x85,0x07,	// REPORT_ID (07)
	0x09,0x80,	// USAGE (RAM Pool Size)
	0x75,0x10,	// REPORT_SIZE (10)
	0x95,0x01,	// REPORT_COUNT (01)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x27,0xFF,0xFF,0x00,0x00,	// LOGICAL_MAXIMUM (00 00 FF FF)
	0x47,0xFF,0xFF,0x00,0x00,	// PHYSICAL_MAXIMUM (00 00 FF FF)
	0xB1,0x02,	// FEATURE (Data,Var,Abs)
	0x09,0x83,	// USAGE (Simultaneous Effects Max)
	0x26,0xFF,0x00,	// LOGICAL_MAXIMUM (00 FF)
	0x46,0xFF,0x00,	// PHYSICAL_MAXIMUM (00 FF)
	0x75,0x08,	// REPORT_SIZE (08)
	0x95,0x01,	// REPORT_COUNT (01)
	0xB1,0x02,	// FEATURE (Data,Var,Abs)
	0x09,0xA9,	// USAGE (Device Managed Pool)
	0x09,0xAA,	// USAGE (Shared Parameter Blocks)
	0x75,0x01,	// REPORT_SIZE (01)
	0x95,0x02,	// REPORT_COUNT (02)
	0x15,0x00,	// LOGICAL_MINIMUM (00)
	0x25,0x01,	// LOGICAL_MAXIMUM (01)
	0x35,0x00,	// PHYSICAL_MINIMUM (00)
	0x45,0x01,	// PHYSICAL_MAXIMUM (01)
	0xB1,0x02,	// FEATURE (Data,Var,Abs)
	0x75,0x06,	// REPORT_SIZE (06)
	0x95,0x01,	// REPORT_COUNT (01)
	0xB1,0x03,	// FEATURE ( Cnst,Var,Abs)
	0xC0,	// END COLLECTION ()
	0xC0,	// END COLLECTION ()
};

//
// This is the default HID descriptor returned by the mini driver
// in response to IOCTL_HID_GET_DEVICE_DESCRIPTOR. The size
// of report descriptor is currently the size of G_DefaultReportDescriptor.
//

HID_DESCRIPTOR              G_DefaultHidDescriptor = {
	0x09,   // length of HID descriptor
	0x21,   // descriptor type == HID  0x21
	0x0100, // hid spec release
	0x00,   // country code == Not Specified
	0x01,   // number of HID class descriptors
	{                                       //DescriptorList[0]
		0x22,                               //report descriptor type 0x22
		sizeof(G_DefaultReportDescriptor)   //total length of report descriptor
	}
};
#pragma endregion

NTSTATUS
VirtualHID_CreateDevice(
	_Inout_ PWDFDEVICE_INIT DeviceInit
)
/*++

Routine Description:

	Worker routine called to create a device and its software resources.

Arguments:

	DeviceInit - Pointer to an opaque init structure. Memory for this
					structure will be freed by the framework when the WdfDeviceCreate
					succeeds. So don't access the structure after that point.

Return Value:

	NTSTATUS

--*/
{
	NTSTATUS                status;
	WDF_OBJECT_ATTRIBUTES   deviceAttributes;
	WDFDEVICE               device;
	PDEVICE_CONTEXT         deviceContext;
	PHID_DEVICE_ATTRIBUTES  hidAttributes;

	KdPrint(("Enter EvtDeviceAdd\n"));
	// Mark ourselves as a filter, which also relinquishes power policy ownership
	//
	WdfFdoInitSetFilter(DeviceInit);

	WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(
		&deviceAttributes,
		DEVICE_CONTEXT);

	status = WdfDeviceCreate(&DeviceInit,
		&deviceAttributes,
		&device);
	if (!NT_SUCCESS(status)) {
		KdPrint(("Error: WdfDeviceCreate failed 0x%x\n", status));
		return status;
	}

	deviceContext = GetDeviceContext(device);
	deviceContext->Device = device;

	hidAttributes = &deviceContext->HidDeviceAttributes;
	RtlZeroMemory(hidAttributes, sizeof(HID_DEVICE_ATTRIBUTES));
	hidAttributes->Size = sizeof(HID_DEVICE_ATTRIBUTES);
	hidAttributes->VendorID = HIDMINI_VID;
	hidAttributes->ProductID = HIDMINI_PID;
	hidAttributes->VersionNumber = HIDMINI_VERSION;

	status = VirtualHID_QueueCreate(device, &deviceContext->DefaultQueue);
	if (!NT_SUCCESS(status)) {
		return status;
	}

	//
	// Use default "HID Descriptor" (hardcoded). We will set the
	// wReportLength memeber of HID descriptor when we read the
	// the report descriptor either from registry or the hard-coded
	// one.
	//
	deviceContext->HidDescriptor = G_DefaultHidDescriptor;

	deviceContext->ReportDescriptor = G_DefaultReportDescriptor;

	status = VirtualHID_CreateRawPdo(device);

	return status;
}

NTSTATUS
VirtualHID_QueueCreate(
	_In_  WDFDEVICE         Device,
	_Out_ WDFQUEUE          *Queue
)
/*++
Routine Description:

This function creates a default, parallel I/O queue to proces IOCTLs
from hidclass.sys.

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

	WDF_IO_QUEUE_CONFIG_INIT_DEFAULT_QUEUE(
		&queueConfig,
		WdfIoQueueDispatchParallel);

#ifdef _KERNEL_MODE
	queueConfig.EvtIoInternalDeviceControl = VirtualHID_EvtIoDeviceControl;
#else
	//
	// HIDclass uses INTERNAL_IOCTL which is not supported by UMDF. Therefore
	// the hidumdf.sys changes the IOCTL type to DEVICE_CONTROL for next stack
	// and sends it down
	//
	queueConfig.EvtIoDeviceControl = EvtIoDeviceControl;
#endif

	status = WdfIoQueueCreate(
		Device,
		&queueConfig,
		NULL,
		&queue);

	if (!NT_SUCCESS(status)) {
		KdPrint(("WdfEventIoQueueCreate failed 0x%x\n", status));
		return status;
	}
	*Queue = queue;
	return status;
}

VOID
VirtualHID_EvtIoDeviceControl(
	_In_  WDFQUEUE          Queue,
	_In_  WDFREQUEST        Request,
	_In_  size_t            OutputBufferLength,
	_In_  size_t            InputBufferLength,
	_In_  ULONG             IoControlCode
)
/*++
Routine Description:

This event callback function is called when the driver receives an

(KMDF) IOCTL_HID_Xxx code when handlng IRP_MJ_INTERNAL_DEVICE_CONTROL
(UMDF) IOCTL_HID_Xxx, IOCTL_UMDF_HID_Xxx when handling IRP_MJ_DEVICE_CONTROL

Arguments:

Queue - A handle to the queue object that is associated with the I/O request

Request - A handle to a framework request object.

OutputBufferLength - The length, in bytes, of the request's output buffer,
if an output buffer is available.

InputBufferLength - The length, in bytes, of the request's input buffer, if
an input buffer is available.

IoControlCode - The driver or system defined IOCTL associated with the request

Return Value:

NTSTATUS

--*/
{
	NTSTATUS                status;
	BOOLEAN                 completeRequest = TRUE;
	WDFDEVICE               device = WdfIoQueueGetDevice(Queue);
	PDEVICE_CONTEXT         deviceContext = NULL;
	PRPDO_DEVICE_DATA       rawDeviceContext = NULL;
	UNREFERENCED_PARAMETER(OutputBufferLength);
	UNREFERENCED_PARAMETER(InputBufferLength);

	deviceContext = GetDeviceContext(device);

	switch (IoControlCode)
	{
	case IOCTL_HID_GET_DEVICE_DESCRIPTOR:   // METHOD_NEITHER
											//
											// Retrieves the device's HID descriptor.
											//
		_Analysis_assume_(deviceContext->HidDescriptor.bLength != 0);
		status = VirtualHID_RequestCopyFromBuffer(Request,
			&deviceContext->HidDescriptor,
			deviceContext->HidDescriptor.bLength);
		break;

	case IOCTL_HID_GET_DEVICE_ATTRIBUTES:   // METHOD_NEITHER
											//
											//Retrieves a device's attributes in a HID_DEVICE_ATTRIBUTES structure.
											//
		status = VirtualHID_RequestCopyFromBuffer(Request,
			&deviceContext->HidDeviceAttributes,
			sizeof(HID_DEVICE_ATTRIBUTES));
		break;

	case IOCTL_HID_GET_REPORT_DESCRIPTOR:   // METHOD_NEITHER
											//
											//Obtains the report descriptor for the HID device.
											//
		status = VirtualHID_RequestCopyFromBuffer(Request,
			deviceContext->ReportDescriptor,
			deviceContext->HidDescriptor.DescriptorList[0].wReportLength);
		break;

	case IOCTL_HID_READ_REPORT:             // METHOD_NEITHER
											//
											// Returns a report from the device into a class driver-supplied
											// buffer.
											//
		rawDeviceContext = deviceContext->RawDeviceContext;
		status = VirtualHID_ForwardRequest(rawDeviceContext->ReadReportQueue, Request, &completeRequest, "IOCTL_HID_READ_REPORT");
		break;

	case IOCTL_HID_WRITE_REPORT:            // METHOD_NEITHER
											//
											// Transmits a class driver-supplied report to the device.
											//
		rawDeviceContext = deviceContext->RawDeviceContext;
		status = VirtualHID_ForwardRequest(rawDeviceContext->WriteReportQueue, Request, &completeRequest, "IOCTL_HID_WRITE_REPORT");
		break;

#ifdef _KERNEL_MODE

	case IOCTL_HID_GET_FEATURE:             // METHOD_OUT_DIRECT

		rawDeviceContext = deviceContext->RawDeviceContext;
		status = VirtualHID_ForwardRequest(rawDeviceContext->GetFeatureQueue, Request, &completeRequest, "IOCTL_HID_GET_FEATURE");
		break;

	case IOCTL_HID_SET_FEATURE:             // METHOD_IN_DIRECT

		rawDeviceContext = deviceContext->RawDeviceContext;
		status = VirtualHID_ForwardRequest(rawDeviceContext->SetFeatureQueue, Request, &completeRequest, "IOCTL_HID_SET_FEATURE");
		break;

	case IOCTL_HID_GET_INPUT_REPORT:        // METHOD_OUT_DIRECT

		rawDeviceContext = deviceContext->RawDeviceContext;
		status = VirtualHID_ForwardRequest(rawDeviceContext->GetInputQueue, Request, &completeRequest, "IOCTL_HID_GET_INPUT_REPORT");
		break;

	case IOCTL_HID_SET_OUTPUT_REPORT:       // METHOD_IN_DIRECT

		rawDeviceContext = deviceContext->RawDeviceContext;
		status = VirtualHID_ForwardRequest(rawDeviceContext->SetOutputQueue, Request, &completeRequest, "IOCTL_HID_SET_OUTPUT_REPORT");
		break;

#else // UMDF specific

		//
		// HID minidriver IOCTL uses HID_XFER_PACKET which contains an embedded pointer.
		//
		//   typedef struct _HID_XFER_PACKET {
		//     PUCHAR reportBuffer;
		//     ULONG  reportBufferLen;
		//     UCHAR  reportId;
		//   } HID_XFER_PACKET, *PHID_XFER_PACKET;
		//
		// UMDF cannot handle embedded pointers when marshalling buffers between processes.
		// Therefore a special driver mshidumdf.sys is introduced to convert such IRPs to
		// new IRPs (with new IOCTL name like IOCTL_UMDF_HID_Xxxx) where:
		//
		//   reportBuffer - passed as one buffer inside the IRP
		//   reportId     - passed as a second buffer inside the IRP
		//
		// The new IRP is then passed to UMDF host and driver for further processing.
		//

	case IOCTL_UMDF_HID_GET_FEATURE:        // METHOD_NEITHER

		status = GetFeature(queueContext, Request);
		break;

	case IOCTL_UMDF_HID_SET_FEATURE:        // METHOD_NEITHER

		status = SetFeature(queueContext, Request);
		break;

	case IOCTL_UMDF_HID_GET_INPUT_REPORT:  // METHOD_NEITHER

		status = GetInputReport(queueContext, Request);
		break;

	case IOCTL_UMDF_HID_SET_OUTPUT_REPORT: // METHOD_NEITHER

		status = SetOutputReport(queueContext, Request);
		break;

#endif // _KERNEL_MODE

	case IOCTL_HID_GET_STRING:                      // METHOD_NEITHER

		status = VirtualHID_GetString(Request);
		break;

	case IOCTL_HID_GET_INDEXED_STRING:              // METHOD_OUT_DIRECT

		status = VirtualHID_GetIndexedString(Request);
		break;

	case IOCTL_HID_SEND_IDLE_NOTIFICATION_REQUEST:  // METHOD_NEITHER
													//
													// This has the USBSS Idle notification callback. If the lower driver
													// can handle it (e.g. USB stack can handle it) then pass it down
													// otherwise complete it here as not inplemented. For a virtual
													// device, idling is not needed.
													//
													// Not implemented. fall through...
													//
	case IOCTL_HID_ACTIVATE_DEVICE:                 // METHOD_NEITHER
	case IOCTL_HID_DEACTIVATE_DEVICE:               // METHOD_NEITHER
	case IOCTL_GET_PHYSICAL_DESCRIPTOR:             // METHOD_OUT_DIRECT
													//
													// We don't do anything for these IOCTLs but some minidrivers might.
													//
													// Not implemented. fall through...
													//
	default:
		KdPrint(("Status not implemented\n"));
		status = STATUS_NOT_IMPLEMENTED;
		break;
	}

	//
	// Complete the request. Information value has already been set by request
	// handlers.
	//
	if (completeRequest) {
		WdfRequestComplete(Request, status);
	}
}

NTSTATUS
VirtualHID_GetStringId(
	_In_  WDFREQUEST        Request,
	_Out_ ULONG            *StringId,
	_Out_ ULONG            *LanguageId
)
/*++

Routine Description:

Helper routine to decode IOCTL_HID_GET_INDEXED_STRING and IOCTL_HID_GET_STRING.

Arguments:

Request - Pointer to Request Packet.

Return Value:

NT status code.

--*/
{
	NTSTATUS                status;
	ULONG                   inputValue;

#ifdef _KERNEL_MODE

	WDF_REQUEST_PARAMETERS  requestParameters;

	//
	// IOCTL_HID_GET_STRING:                      // METHOD_NEITHER
	// IOCTL_HID_GET_INDEXED_STRING:              // METHOD_OUT_DIRECT
	//
	// The string id (or string index) is passed in Parameters.DeviceIoControl.
	// Type3InputBuffer. However, Parameters.DeviceIoControl.InputBufferLength
	// was not initialized by hidclass.sys, therefore trying to access the
	// buffer with WdfRequestRetrieveInputMemory will fail
	//
	// Another problem with IOCTL_HID_GET_INDEXED_STRING is that METHOD_OUT_DIRECT
	// expects the input buffer to be Irp->AssociatedIrp.SystemBuffer instead of
	// Type3InputBuffer. That will also fail WdfRequestRetrieveInputMemory.
	//
	// The solution to the above two problems is to get Type3InputBuffer directly
	//
	// Also note that instead of the buffer's content, it is the buffer address
	// that was used to store the string id (or index)
	//

	WDF_REQUEST_PARAMETERS_INIT(&requestParameters);
	WdfRequestGetParameters(Request, &requestParameters);

	inputValue = PtrToUlong(
		requestParameters.Parameters.DeviceIoControl.Type3InputBuffer);

	status = STATUS_SUCCESS;

#else

	WDFMEMORY               inputMemory;
	size_t                  inputBufferLength;
	PVOID                   inputBuffer;

	//
	// mshidumdf.sys updates the IRP and passes the string id (or index) through
	// the input buffer correctly based on the IOCTL buffer type
	//

	status = WdfRequestRetrieveInputMemory(Request, &inputMemory);
	if (!NT_SUCCESS(status)) {
		KdPrint(("WdfRequestRetrieveInputMemory failed 0x%x\n", status));
		return status;
	}
	inputBuffer = WdfMemoryGetBuffer(inputMemory, &inputBufferLength);

	//
	// make sure buffer is big enough.
	//
	if (inputBufferLength < sizeof(ULONG))
	{
		status = STATUS_INVALID_BUFFER_SIZE;
		KdPrint(("GetStringId: invalid input buffer. size %d, expect %d\n",
			(int)inputBufferLength, (int)sizeof(ULONG)));
		return status;
	}

	inputValue = (*(PULONG)inputBuffer);

#endif

	//
	// The least significant two bytes of the INT value contain the string id.
	//
	*StringId = (inputValue & 0x0ffff);

	//
	// The most significant two bytes of the INT value contain the language
	// ID (for example, a value of 1033 indicates English).
	//
	*LanguageId = (inputValue >> 16);

	return status;
}

NTSTATUS
VirtualHID_GetIndexedString(
	_In_  WDFREQUEST        Request
)
/*++

Routine Description:

Handles IOCTL_HID_GET_INDEXED_STRING

Arguments:

Request - Pointer to Request Packet.

Return Value:

NT status code.

--*/
{
	NTSTATUS                status;
	ULONG                   languageId, stringIndex;

	status = VirtualHID_GetStringId(Request, &stringIndex, &languageId);

	// While we don't use the language id, some minidrivers might.
	//
	UNREFERENCED_PARAMETER(languageId);

	if (NT_SUCCESS(status)) {
		if (stringIndex != VHIDMINI_DEVICE_STRING_INDEX)
		{
			status = STATUS_INVALID_PARAMETER;
			KdPrint(("GetString: unkown string index %d\n", stringIndex));
			return status;
		}

		status = VirtualHID_RequestCopyFromBuffer(Request, VHIDMINI_DEVICE_STRING, sizeof(VHIDMINI_DEVICE_STRING));
	}
	return status;
}

NTSTATUS
VirtualHID_GetString(
	_In_  WDFREQUEST        Request
)
/*++

Routine Description:

Handles IOCTL_HID_GET_STRING.

Arguments:

Request - Pointer to Request Packet.

Return Value:

NT status code.

--*/
{
	NTSTATUS                status;
	ULONG                   languageId, stringId;
	size_t                  stringSizeCb;
	PWSTR                   string;

	status = VirtualHID_GetStringId(Request, &stringId, &languageId);

	// While we don't use the language id, some minidrivers might.
	//
	UNREFERENCED_PARAMETER(languageId);

	if (!NT_SUCCESS(status)) {
		return status;
	}

	switch (stringId) {
	case HID_STRING_ID_IMANUFACTURER:
		stringSizeCb = sizeof(VHIDMINI_MANUFACTURER_STRING);
		string = VHIDMINI_MANUFACTURER_STRING;
		break;
	case HID_STRING_ID_IPRODUCT:
		stringSizeCb = sizeof(VHIDMINI_PRODUCT_STRING);
		string = VHIDMINI_PRODUCT_STRING;
		break;
	case HID_STRING_ID_ISERIALNUMBER:
		stringSizeCb = sizeof(VHIDMINI_SERIAL_NUMBER_STRING);
		string = VHIDMINI_SERIAL_NUMBER_STRING;
		break;
	default:
		status = STATUS_INVALID_PARAMETER;
		KdPrint(("GetString: unkown string id %d\n", stringId));
		return status;
	}

	status = VirtualHID_RequestCopyFromBuffer(Request, string, stringSizeCb);
	return status;
}

NTSTATUS
VirtualHID_ForwardRequest(
	_In_  WDFQUEUE    Queue,
	_In_  WDFREQUEST        Request,
	_Always_(_Out_)
	BOOLEAN*          CompleteRequest,
	_In_ char *ioctlName
)
/*++

Routine Description:

Handles IOCTL_HID_READ_REPORT for the HID collection. Normally the request
will be forwarded to a manual queue for further process. In that case, the
caller should not try to complete the request at this time, as the request
will later be retrieved back from the manually queue and completed there.
However, if for some reason the forwarding fails, the caller still need
to complete the request with proper error code immediately.

Arguments:

QueueContext - The object context associated with the queue

Request - Pointer to  Request Packet.

CompleteRequest - A boolean output value, indicating whether the caller
should complete the request or not

Return Value:

NT status code.

--*/
{
	NTSTATUS                status;

	//PHID_XFER_PACKET transferPacket;
	//int cmp;
	KdPrint(("ReadReport %s\n", ioctlName));

	//cmp = strcmp("IOCTL_HID_READ_REPORT", ioctlName);
	//if (cmp != 0)
	//{
	//	transferPacket = (PHID_XFER_PACKET)WdfRequestWdmGetIrp(Request)->UserBuffer;

	//	KdPrint(("id %d\n", transferPacket->reportId));

	//	for (int i = 0; i < (int)transferPacket->reportBufferLen; i++)
	//	{
	//		KdPrint(("%d 0x%x\n ", i, (unsigned char)*(transferPacket->reportBuffer + i)));
	//	}

	//	cmp = strcmp("IOCTL_HID_GET_FEATURE", ioctlName);

	//	if (cmp == 0)
	//	{
	//		KdPrint(("GetFeature!"));
	//		if (7 == transferPacket->reportId)
	//		{
	//			KdPrint(("Its seven!!\n"));
	//			KdPrint(("%p\n", transferPacket->reportBuffer));
	//			unsigned short * pRamPool = (unsigned short *)(transferPacket->reportBuffer + sizeof(transferPacket->reportId));
	//			KdPrint(("%p\n", pRamPool));
	//			*pRamPool = 0xFFFF;

	//			unsigned char * maxEffect = (unsigned char *)pRamPool + sizeof(unsigned short);
	//			KdPrint(("%p %d\n", maxEffect, sizeof(unsigned short)));
	//			*maxEffect = 0xFF;

	//			*(++maxEffect) = 0x81;

	//			for (int i = 0; i < (int)transferPacket->reportBufferLen; i++)
	//			{
	//				KdPrint(("%d 0x%x\n ", i, (unsigned char)*(transferPacket->reportBuffer + i)));
	//			}
	//		}
	//	}
	//	WdfRequestSetInformation(Request, transferPacket->reportBufferLen);
	//	*CompleteRequest = TRUE;
	//	return STATUS_SUCCESS;
	//}

	//
	// forward the request to manual queue
	//
	status = WdfRequestForwardToIoQueue(
		Request,
		Queue);
	if (!NT_SUCCESS(status)) {
		KdPrint(("WdfRequestForwardToIoQueue failed with 0x%x\n", status));
		*CompleteRequest = TRUE;
	}
	else {
		*CompleteRequest = FALSE;
	}

	return status;
}

NTSTATUS
VirtualHID_RequestCopyFromBuffer(
	_In_  WDFREQUEST        Request,
	_In_  PVOID             SourceBuffer,
	_When_(NumBytesToCopyFrom == 0, __drv_reportError(NumBytesToCopyFrom cannot be zero))
	_In_  size_t            NumBytesToCopyFrom
)
/*++

Routine Description:

A helper function to copy specified bytes to the request's output memory

Arguments:

Request - A handle to a framework request object.

SourceBuffer - The buffer to copy data from.

NumBytesToCopyFrom - The length, in bytes, of data to be copied.

Return Value:

NTSTATUS

--*/
{
	NTSTATUS                status;
	WDFMEMORY               memory;
	size_t                  outputBufferLength;

	status = WdfRequestRetrieveOutputMemory(Request, &memory);
	if (!NT_SUCCESS(status)) {
		KdPrint(("WdfRequestRetrieveOutputMemory failed 0x%x\n", status));
		return status;
	}

	WdfMemoryGetBuffer(memory, &outputBufferLength);
	if (outputBufferLength < NumBytesToCopyFrom) {
		status = STATUS_INVALID_BUFFER_SIZE;
		KdPrint(("RequestCopyFromBuffer: buffer too small. Size %d, expect %d\n",
			(int)outputBufferLength, (int)NumBytesToCopyFrom));
		return status;
	}

	status = WdfMemoryCopyFromBuffer(memory,
		0,
		SourceBuffer,
		NumBytesToCopyFrom);
	if (!NT_SUCCESS(status)) {
		KdPrint(("WdfMemoryCopyFromBuffer failed 0x%x\n", status));
		return status;
	}

	WdfRequestSetInformation(Request, NumBytesToCopyFrom);
	return status;
}
NTSTATUS
VirtualHID_RequestCopyToBuffer(
	_In_  WDFREQUEST        Request,
	_In_  PVOID             DestinationBuffer,
	_When_(NumBytesToCopyTo == 0, __drv_reportError(NumBytesToCopyTo cannot be zero))
	_In_  size_t            bufferSize
)
/*++

Routine Description:

A helper function to copy specified bytes to the request's output memory

Arguments:

Request - A handle to a framework request object.

SourceBuffer - The buffer to copy data from.

NumBytesToCopyFrom - The length, in bytes, of data to be copied.

Return Value:

NTSTATUS

--*/
{
	NTSTATUS                status;
	WDFMEMORY               memory;
	size_t                  inputBufferLength;

	status = WdfRequestRetrieveInputMemory(Request, &memory);
	if (!NT_SUCCESS(status)) {
		KdPrint(("WdfRequestRetrieveInputMemory failed 0x%x\n", status));
		return status;
	}

	WdfMemoryGetBuffer(memory, &inputBufferLength);
	if (inputBufferLength > bufferSize) {
		status = STATUS_INVALID_BUFFER_SIZE;
		KdPrint(("RequestCopyFromBuffer: buffer too small. Size %d, expect %d\n",
			(int)inputBufferLength, (int)bufferSize));
		return status;
	}

	status = WdfMemoryCopyToBuffer(memory,
		0,
		DestinationBuffer,
		inputBufferLength);
	if (!NT_SUCCESS(status)) {
		KdPrint(("WdfMemoryCopyFromBuffer failed 0x%x\n", status));
		return status;
	}

	WdfRequestSetInformation(Request, inputBufferLength);
	return status;
}