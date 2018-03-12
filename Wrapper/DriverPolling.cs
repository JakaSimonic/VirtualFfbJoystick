using System;
using System.Timers;

namespace VHClibWrapper
{
    public class DriverPolling : IDisposable
    {
        private DriverCommunication[] driverCommunication;
        private Timer pollTimer;
        private bool disposed = false;

        public event EventHandler<HidEventArg> HidEvent;

        public bool TestPassed { get; private set; } = false;

        public DriverPolling()
        {
            if (NativeMethods.OpenFile())
            {
                throw new Exception("File failed to open");
            }

            string testMsg = string.Format("VirtualHID test time :{0}", DateTime.Now.TimeOfDay);
            NativeMethods.TestWrite(testMsg);
            TestPassed = string.Compare(NativeMethods.TestRead(), testMsg) == 0;
            if (!TestPassed)
            {
                throw new Exception("Driver test failed!");
            }

            driverCommunication = MapDriverCommunication();
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
            foreach (var dc in driverCommunication)
            {
                byte[] buffer = dc.DriverRead();

                if (buffer == null)
                {
                    continue;
                }
                else
                {
                    int returnedBytes = buffer.Length;

                    HidEventArg hidArg = new HidEventArg(buffer, dc.HidIoctlEnum);
                    HidEvent(this, hidArg);

                    if (dc.DriverWrite != null)
                    {
                        if (returnedBytes == hidArg.buffer.Length)
                        {
                            dc.DriverWrite(hidArg.buffer);
                        }
                        else
                        {
                            throw new Exception(String.Format("DriverPolling buffer length error! start: {0} returned: {1}", returnedBytes, hidArg.buffer.Length));
                        }
                    }
                }
            }
        }

        public void SendReadReport(byte[] buffer)
        {
            if (NativeMethods.Read_ReadReportQueue() != null)
            {
                NativeMethods.Write_ReadReportQueue(buffer);
            }
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

        private DriverCommunication[] MapDriverCommunication()
        {
            return new DriverCommunication[]
            {
                new DriverCommunication(NativeMethods.Read_SetFeatureQueue, null, HidIoctlEnum.SetFeature),
                new DriverCommunication(NativeMethods.Read_GetFeatureQueue, NativeMethods.Write_GetFeatureQueue, HidIoctlEnum.GetFeature),
                new DriverCommunication(NativeMethods.Read_WriteReportQueue, null, HidIoctlEnum.WriteReport),
                new DriverCommunication(NativeMethods.Read_GetInputReportQueue, NativeMethods.Write_GetInputReportQueue, HidIoctlEnum.GetInputReport),
                new DriverCommunication(NativeMethods.Read_SetOutputReportQueue, null, HidIoctlEnum.SetOutputReport)
            };
        }

        private class DriverCommunication
        {
            internal DriverCommunication(Func<byte[]> _DriverRead, Action<byte[]> _DriverWrite, HidIoctlEnum _HidIoctlEnum)
            {
                DriverRead = _DriverRead;
                DriverWrite = _DriverWrite;
                HidIoctlEnum = _HidIoctlEnum;
            }

            internal Func<byte[]> DriverRead;
            internal Action<byte[]> DriverWrite;
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