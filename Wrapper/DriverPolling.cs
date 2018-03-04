using System;
using System.Timers;

namespace VHClibWrapper
{
    internal class DriverPolling : IDisposable
    {
        private bool fileOpened;
        private Timer pollTimer;
        private bool disposed = false;
        private static byte[] buffer;
        private static int bufferSize;
        private const uint STATUS_PIPE_EMPTY = 0xC00000D9;
        private const uint STATUS_SUCCESS = 1;

        private delegate uint DriverReadDelegate(byte[] buffer, int bufferSize, ref uint returnSize);

        private delegate uint DriverWriteDelegate(byte[] buffer, int bytesToWrite);

        public event EventHandler<HidEventArg> SetFeature;

        public event EventHandler<HidEventArg> GetFeature;

        public event EventHandler<HidEventArg> WriteReport;

        public event EventHandler<HidEventArg> GetInputReport;

        public event EventHandler<HidEventArg> SetOutputReport;

        private static Tuple<DriverReadDelegate, DriverWriteDelegate, EventHandler<HidEventArg>>[] pollingMemebers;

        public DriverPolling(int pollingInterval, int buffSize)
        {
            fileOpened = NativeMethods.OpenFile();
            if (!fileOpened)
            {
                throw new Exception("File failed to open");
            }

            bufferSize = buffSize;
            buffer = new byte[bufferSize];

            pollingMemebers = InitPollingMembers();

            pollTimer = new System.Timers.Timer();
            pollTimer.Interval = pollingInterval;

            // Hook up the Elapsed event for the timer.
            pollTimer.Elapsed += OnPollEvent;

            // Have the timer fire repeated events (true is the default)
            pollTimer.AutoReset = true;

            // Start the timer
            pollTimer.Start();

            Console.WriteLine("Press the Enter key to exit the program at any time... ");
            Console.ReadLine();
        }

        ~DriverPolling()
        {
            Dispose(false);
        }

        private static void OnPollEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            uint status;
            uint returnedBytes = 0;

            foreach (var tuple in pollingMemebers)
            {
                status = tuple.Item1()
            }
            status = NativeMethods.ReadGetFeatureReport(buffer, bufferSize, ref returnedBytes);
        }

        public uint CompleteGetFeature(byte[] buffer)
        {
            return NativeMethods.WriteGetFeatureReport(buffer, buffer.Length);
        }

        public uint CompleteReadReport(byte[] buffer)
        {
            return NativeMethods.WriteReadReport(buffer, buffer.Length);
        }

        public uint CompleteGetInputReport(byte[] buffer)
        {
            return NativeMethods.WriteGetInputReport(buffer, buffer.Length);
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

        private void ProcessRequest(uint status, int responseBufferSize, EventHandler<HidEventArg> EventItem, DriverWriteDelegate WriteFunc)
        {
            if (STATUS_PIPE_EMPTY == status)
            {
                return;
            }
            else if (STATUS_SUCCESS == status)
            {
                HidEventArg hidArg = new HidEventArg();
                Array.Copy(buffer, hidArg.buffer, responseBufferSize);
                EventItem(this, hidArg);

                if (WriteFunc != null)
                {
                    if(responseBufferSize==hidArg.buffer.Length)
                    {
                        WriteFunc(hidArg.buffer, hidArg.buffer.Length);
                    }
                }
            }
        }

        private Tuple<DriverReadDelegate, DriverWriteDelegate, EventHandler<HidEventArg>>[] InitPollingMembers()
        {
            Tuple<DriverReadDelegate, DriverWriteDelegate, EventHandler<HidEventArg>>[] pollingMemebersTemp =
            {
                Tuple.Create<DriverReadDelegate, DriverWriteDelegate, EventHandler<HidEventArg>>(NativeMethods.ReadSetFeatureReport, null, SetFeature),
                Tuple.Create<DriverReadDelegate, DriverWriteDelegate, EventHandler<HidEventArg>>(NativeMethods.ReadGetFeatureReport, NativeMethods.WriteGetFeatureReport, GetFeature),
                Tuple.Create<DriverReadDelegate, DriverWriteDelegate, EventHandler<HidEventArg>>(NativeMethods.ReadWriteReport, null, WriteReport),
                Tuple.Create<DriverReadDelegate, DriverWriteDelegate, EventHandler<HidEventArg>>(NativeMethods.ReadGetInputReportReport, NativeMethods.WriteGetInputReport, GetInputReport),
                Tuple.Create<DriverReadDelegate, DriverWriteDelegate, EventHandler<HidEventArg>>(NativeMethods.ReadSetOutputReport, null, SetOutputReport),
            };

            return pollingMemebersTemp;
        }
    }
}