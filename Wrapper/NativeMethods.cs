using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace VHClibWrapper
{
    internal static class NativeMethods
    {
        private const uint STATUS_NO_MORE_ENTRIES = 259;
        private const uint STATUS_SUCCESS = 1;
        internal const int BUFFER_MAX_SIZE = 256;


        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern uint CloseDriverFile();

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern uint OpenDriverFile();

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern uint TestRead(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            uint bufferLength,
            ref uint returnedBytes);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern uint TestWrite(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            uint bufferLength);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern uint Read_GetFeatureQueue(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bufferSize,
            ref uint returnedBytes);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern uint Read_GetInputReportQueue(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bufferSize,
            ref uint returnedBytes);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern uint Read_SetFeatureQueue(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bufferSize,
            ref uint returnedBytes);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern uint Read_WriteReportQueue(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bufferSize,
            ref uint returnedBytes);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern uint Read_ReadReportQueue(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bufferSize,
            ref uint returnedBytes);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern uint Read_SetOutputReportQueue(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bufferSize,
            ref uint returnedBytes);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern uint Write_GetFeatureQueue(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bytesToWrite);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern uint Write_ReadReportQueue(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bytesToWrite);

        [DllImport("VHClib.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        private static extern uint Write_GetInputReportQueue(
            [param: MarshalAs(UnmanagedType.LPArray)]
            byte[] buffer,
            int bytesToWrite);

        public static void Write_GetFeatureQueue(byte[] buffer)
        {
            uint res = Write_GetFeatureQueue(buffer, buffer.Length);

            if (res != STATUS_SUCCESS)
            {
                throw new Exception(String.Format("Write_GetFeatureQueue returned: {0}", Marshal.GetLastWin32Error()));
            }
        }

        public static void Write_ReadReportQueue(byte[] buffer)
        {
            uint res = Write_ReadReportQueue(buffer, buffer.Length);

            if (res != STATUS_SUCCESS)
            {
                throw new Exception(String.Format("Write_ReadReportQueue returned: {0}", Marshal.GetLastWin32Error()));
            }
        }

        public static void Write_GetInputReportQueue(byte[] buffer)
        {
            uint res = Write_GetInputReportQueue(buffer, buffer.Length);

            if (res != STATUS_SUCCESS)
            {
                throw new Exception(String.Format("Write_GetInputReportQueue returned: {0}", Marshal.GetLastWin32Error()));
            }
        }

        public static byte[] Read_GetFeatureQueue()
        {
            byte[] buffer = new byte[BUFFER_MAX_SIZE];
            uint returnedBytes = 0;

            uint res = Read_GetFeatureQueue(buffer, buffer.Length, ref returnedBytes);
            if (res == STATUS_SUCCESS)
            {
                return buffer.Take((int)returnedBytes).ToArray();
            }
            else
            {
                int lastError = Marshal.GetLastWin32Error();
                if (lastError == STATUS_NO_MORE_ENTRIES)
                {
                    return null;
                }
                else
                {
                    throw new Exception(String.Format("Read_GetFeatureQueue returned {0}", lastError));
                }
            }
        }

        public static byte[] Read_GetInputReportQueue()
        {
            byte[] buffer = new byte[BUFFER_MAX_SIZE];
            uint returnedBytes = 0;

            uint res = Read_GetInputReportQueue(buffer, buffer.Length, ref returnedBytes);
            if (res == STATUS_SUCCESS)
            {
                return buffer.Take((int)returnedBytes).ToArray();
            }
            else
            {
                int lastError = Marshal.GetLastWin32Error();
                if (lastError == STATUS_NO_MORE_ENTRIES)
                {
                    return null;
                }
                else
                {
                    throw new Exception(String.Format("Read_GetFeatureQueue returned {0}", lastError));
                }
            }
        }

        public static byte[] Read_SetFeatureQueue()
        {
            byte[] buffer = new byte[BUFFER_MAX_SIZE];
            uint returnedBytes = 0;

            uint res = Read_SetFeatureQueue(buffer, buffer.Length, ref returnedBytes);
            if (res == STATUS_SUCCESS)
            {
                return buffer.Take((int)returnedBytes).ToArray();
            }
            else
            {
                int lastError = Marshal.GetLastWin32Error();
                if (lastError == STATUS_NO_MORE_ENTRIES)
                {
                    return null;
                }
                else
                {
                    throw new Exception(String.Format("Read_GetFeatureQueue returned {0}", lastError));
                }
            }
        }

        public static byte[] Read_WriteReportQueue()
        {
            byte[] buffer = new byte[BUFFER_MAX_SIZE];
            uint returnedBytes = 0;

            uint res = Read_WriteReportQueue(buffer, buffer.Length, ref returnedBytes);
            if (res == STATUS_SUCCESS)
            {
                return buffer.Take((int)returnedBytes).ToArray();
            }
            else
            {
                int lastError = Marshal.GetLastWin32Error();
                if (lastError == STATUS_NO_MORE_ENTRIES)
                {
                    return null;
                }
                else
                {
                    throw new Exception(String.Format("Read_GetFeatureQueue returned {0}", lastError));
                }
            }
        }

        public static byte[] Read_ReadReportQueue()
        {
            byte[] buffer = new byte[BUFFER_MAX_SIZE];
            uint returnedBytes = 0;

            uint res = Read_ReadReportQueue(buffer, buffer.Length, ref returnedBytes);
            if (res == STATUS_SUCCESS)
            {
                return buffer.Take((int)returnedBytes).ToArray();
            }
            else
            {
                int lastError = Marshal.GetLastWin32Error();
                if (lastError == STATUS_NO_MORE_ENTRIES)
                {
                    return null;
                }
                else
                {
                    throw new Exception(String.Format("Read_GetFeatureQueue returned {0}", lastError));
                }
            }
        }

        public static byte[] Read_SetOutputReportQueue()
        {
            byte[] buffer = new byte[BUFFER_MAX_SIZE];
            uint returnedBytes = 0;

            uint res = Read_SetOutputReportQueue(buffer, buffer.Length, ref returnedBytes);
            if (res == STATUS_SUCCESS)
            {
                return buffer.Take((int)returnedBytes).ToArray();
            }
            else
            {
                int lastError = Marshal.GetLastWin32Error();
                if (lastError == STATUS_NO_MORE_ENTRIES)
                {
                    return null;
                }
                else
                {
                    throw new Exception(String.Format("Read_GetFeatureQueue returned {0}", lastError));
                }
            }
        }

        public static uint TestWrite(string testMsg)
        {
            byte[] testArray = Encoding.ASCII.GetBytes(testMsg);
            return TestWrite(testArray, (uint)testArray.Length);
        }

        public static string TestRead()
        {
            byte[] testArray = new byte[BUFFER_MAX_SIZE];
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