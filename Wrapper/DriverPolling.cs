using System;
using System.Timers;

namespace VHClibWrapper
{
    public class DriverPolling : IDisposable
    {
        private ReactTable[] reactTable;
        private Timer pollTimer;
        private bool disposed = false;

        public event EventHandler<HidEventArg> HidEvent;

        public bool Connected { get; private set; } = false;

        public DriverPolling()
        {
            reactTable = CreateReactTable();
        }

        ~DriverPolling()
        {
            Dispose(false);
        }

        public void StartPolling(int pollingInterval)
        {
            if (!Connected)
            {
                if (!NativeMethods.OpenFile())
                {
                    throw new Exception("File failed to open");
                }

                string testMsg = string.Format("VirtualHID test time :{0}", DateTime.Now.TimeOfDay);
                NativeMethods.TestWrite(testMsg);
                bool test = string.Compare(NativeMethods.TestRead(), testMsg) == 0;
                if (!test)
                {
                    throw new Exception("Driver test failed!");
                }

                pollTimer = new System.Timers.Timer();
                pollTimer.Interval = pollingInterval;

                // Hook up the Elapsed event for the timer.
                pollTimer.Elapsed += OnPollEvent;

                // Have the timer fire repeated events (true is the default)
                pollTimer.AutoReset = true;

                // Start the timer
                pollTimer.Start();

                Connected = test;
            }
        }

        public void StopPolling()
        {
            if (Connected)
            {
                Connected = false;
                pollTimer.Enabled = false;
                pollTimer.Stop();
                NativeMethods.CloseFile();
            }
        }

        private void OnPollEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            foreach (var re in reactTable)
            {
                byte[] buffer = re.DriverRead();

                if (buffer == null)
                {
                    continue;
                }
                else
                {
                    int returnedBytes = buffer.Length;

                    HidEventArg hidArg = new HidEventArg(buffer, re.HidIoctlEnum);
                    HidEvent(this, hidArg);
                    Console.WriteLine(BitConverter.ToString(hidArg.buffer));

                    if (re.DriverWrite != null)
                    {
                        if (returnedBytes == hidArg.buffer.Length)
                        {
                            re.DriverWrite(hidArg.buffer);
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
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private ReactTable[] CreateReactTable()
        {
            return new ReactTable[]
            {
                new ReactTable(NativeMethods.Read_WriteReportQueue, null, HidIoctlEnum.WriteReport),
                new ReactTable(NativeMethods.Read_SetFeatureQueue, null, HidIoctlEnum.SetFeature),
                new ReactTable(NativeMethods.Read_GetFeatureQueue, NativeMethods.Write_GetFeatureQueue, HidIoctlEnum.GetFeature),
                new ReactTable(NativeMethods.Read_WriteReportQueue, null, HidIoctlEnum.WriteReport),
                new ReactTable(NativeMethods.Read_GetInputReportQueue, NativeMethods.Write_GetInputReportQueue, HidIoctlEnum.GetInputReport),
                new ReactTable(NativeMethods.Read_SetOutputReportQueue, null, HidIoctlEnum.SetOutputReport)
            };
        }

        private class ReactTable
        {
            internal ReactTable(Func<byte[]> _DriverRead, Action<byte[]> _DriverWrite, HidIoctlEnum _HidIoctlEnum)
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