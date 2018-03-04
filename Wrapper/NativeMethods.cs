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
        public static extern uint ReadGetFeatureReport(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bufferSize,
            ref uint returnedBytes);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint ReadGetInputReportReport(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bufferSize,
            ref uint returnedBytes);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint ReadSetFeatureReport(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bufferSize,
            ref uint returnedBytes);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint ReadWriteReport(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bufferSize,
            ref uint returnedBytes);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint ReadReadReport(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bufferSize,
            ref uint returnedBytes);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint ReadSetOutputReport(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bufferSize,
            ref uint returnedBytes);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint WriteGetFeatureReport(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bytesToWrite);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint WriteReadReport(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bytesToWrite);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint WriteGetInputReport(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bytesToWrite);

        public static uint TestWrite(string testString)
        {
            byte[] testArray = Encoding.ASCII.GetBytes(testString);
            return TestWrite(testArray, (uint)testArray.Length);
        }

        public static string TestRead()
        {
            byte[] testArray = new byte[256];
            uint retLen = 0;
            uint result = TestRead(testArray, (uint)testArray.Length, ref retLen);
            return Encoding.UTF8.GetString(testArray, 0, (int)retLen);
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