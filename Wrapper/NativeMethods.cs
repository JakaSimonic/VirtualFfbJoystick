using System;
using System.Runtime.InteropServices;
using System.Text;

namespace VHClibWrapper
{
    internal static class NativeMethods
    {
        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint CloseDriverFile();

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint OpenDriverFile();

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint TestRead(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            uint bufferLength,
            ref uint returnedBytes);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint TestWrite(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            uint bufferLength);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint Read_GetFeatureQueue(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bufferSize,
            ref uint returnedBytes);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint Read_GetInputReportQueue(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bufferSize,
            ref uint returnedBytes);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint Read_SetFeatureQueue(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bufferSize,
            ref uint returnedBytes);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint Read_WriteReportQueue(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bufferSize,
            ref uint returnedBytes);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint Read_ReadReportQueue(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bufferSize,
            ref uint returnedBytes);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint Read_SetOutputReportQueue(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bufferSize,
            ref uint returnedBytes);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint Write_GetFeatureQueue(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bytesToWrite);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint Write_ReadReportQueue(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bytesToWrite);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint Write_GetInputReportQueue(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bytesToWrite);

        public static uint TestWrite(string testMsg)
        {
            byte[] testArray = Encoding.ASCII.GetBytes(testMsg);
            return TestWrite(testArray, (uint)testArray.Length);
        }

        public static uint TestRead(out string testMsg)
        {
            byte[] testArray = new byte[256];
            uint retLen = 0;

            uint result = TestRead(testArray, (uint)testArray.Length, ref retLen);

            testMsg = Encoding.UTF8.GetString(testArray, 0, (int)retLen);

            return result;
        }

        public static bool OpenFile()
        {
            return Convert.ToBoolean(OpenDriverFile());
        }

        public static bool CloseFile()
        {
            return Convert.ToBoolean(CloseDriverFile());
        }
    }
}