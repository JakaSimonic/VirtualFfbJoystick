using System;
using System.Timers;

namespace VHClibWrapper
{
    public class DriverPolling : IDisposable
    {
        private byte[] buffer;
        private MethodChains[] chains;

        private bool fileOpened;
        private Timer pollTimer;
        private bool disposed = false;

        private const int BUFFER_SIZE = 256;
        private const uint STATUS_PIPE_EMPTY = 0xC00000D9;
        private const uint STATUS_SUCCESS = 1;
        private const uint STATUS_CANCELLED = 0xC0000120;
        private const uint STATUS_BUFFER_TOO_SMALL = 0xC0000023;

        private delegate uint DriverReadDelegate(byte[] buffer, int bufferSize, ref uint returnSize);

        private delegate uint DriverWriteDelegate(byte[] buffer, int bytesToWrite);

        public event EventHandler<HidEventArg> HidEvent;


        public DriverPolling()
        {
            fileOpened = NativeMethods.OpenFile();
            if (!fileOpened)
            {
                throw new Exception("File failed to open");
            }

            buffer = new byte[BUFFER_SIZE];

            chains = InitMethodChains();
        }

        ~DriverPolling()
        {
            Dispose(false);
        }

        public void StartPolling(int pollingInterval)
        {
            pollTimer = new System.Timers.Timer();
            pollTimer.Interval = pollingInterval;

            // Hook up the Elapsed event for the timer.
            pollTimer.Elapsed += OnPollEvent;

            // Have the timer fire repeated events (true is the default)
            pollTimer.AutoReset = true;

            // Start the timer
            pollTimer.Start();
        }

        public void StopPolling()
        {
            pollTimer.Stop();
        }

        private void OnPollEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            uint status;
            uint returnedBytes = 0;

            status = NativeMethods.Read_GetFeatureQueue(buffer, buffer.Length, ref returnedBytes);
            foreach (var chain in chains)
            {
                //status = chain.DriverRead(buffer, buffer.Length, ref returnedBytes);

                if (STATUS_PIPE_EMPTY == status)
                {
                    return;
                }
                else if (STATUS_SUCCESS == status)
                {
                    HidEventArg hidArg = new HidEventArg(buffer, chain.HidIoctlEnum);
                    HidEvent(this, hidArg);

                    if (chain.DriverWrite != null)
                    {
                        if (returnedBytes == hidArg.buffer.Length)
                        {
                            uint bytesWritten = chain.DriverWrite(hidArg.buffer, hidArg.buffer.Length);
                            if (bytesWritten != hidArg.buffer.Length)
                            {
                                throw new Exception(String.Format("DriverPolling write error! sent: {0} written {1}", hidArg.buffer.Length, bytesWritten));
                            }
                        }
                        else
                        {
                            throw new Exception(String.Format("DriverPolling buffer length error! start: {0} returned: {1}", returnedBytes, hidArg.buffer.Length));

                        }
                    }
                }
            }
        }

        public uint CompleteGetFeature(byte[] buffer)
        {
            return NativeMethods.Write_ReadReportQueue(buffer, buffer.Length);
        }

        public uint CompleteReadReport(byte[] buffer)
        {
            return NativeMethods.Write_ReadReportQueue(buffer, buffer.Length);
        }

        public uint CompleteGetInputReport(byte[] buffer)
        {
            return NativeMethods.Write_GetInputReportQueue(buffer, buffer.Length);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                }
            }
            //dispose unmanaged resources
            pollTimer.Stop();
            pollTimer.Dispose();
            NativeMethods.CloseFile();
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private MethodChains[] InitMethodChains()
        {
            return new MethodChains[] 
            {
                new MethodChains(NativeMethods.Read_SetFeatureQueue, null, HidIoctlEnum.SetFeature),
                new MethodChains(NativeMethods.Read_GetFeatureQueue, NativeMethods.Write_GetFeatureQueue, HidIoctlEnum.GetFeature),
                new MethodChains(NativeMethods.Read_WriteReportQueue, null, HidIoctlEnum.WriteReport),
                new MethodChains(NativeMethods.Read_GetInputReportQueue, NativeMethods.Write_GetInputReportQueue, HidIoctlEnum.GetInputReport),
                new MethodChains(NativeMethods.Read_SetOutputReportQueue, null, HidIoctlEnum.SetOutputReport),
            };
        }

        private class MethodChains
        {
            internal MethodChains(DriverReadDelegate _DriverRead, DriverWriteDelegate _DriverWrite, HidIoctlEnum _HidIoctlEnum)
            {
                DriverRead= _DriverRead;
                DriverWrite= _DriverWrite;
                HidIoctlEnum= _HidIoctlEnum;
            }
            internal DriverReadDelegate DriverRead;
            internal DriverWriteDelegate DriverWrite;
            internal HidIoctlEnum HidIoctlEnum;
        }
    }

    public enum HidIoctlEnum
    {
        SetFeature,
        GetFeature,
        WriteReport,
        GetInputReport,
        SetOutputReport
    }
}