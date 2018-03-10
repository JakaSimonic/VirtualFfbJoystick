using System;
using VHClibWrapper;

namespace AirJoy
{
    internal class Program
    {
        private static FfbWrapper ffbWrapper;

        private static void Main(string[] args)
        {
            ffbWrapper = new FfbWrapper();
            DriverPolling driverPoll = new DriverPolling();
            driverPoll.HidEvent += DriverPoll;

            driverPoll.StartPolling(30);

            while (true) ;
        }

        private static void DriverPoll(object sender, HidEventArg e)
        {
            //Console.WriteLine(BitConverter.ToString(e.buffer));
            byte[] buffer = e.buffer;
            int initialBufferLength = buffer.Length;
            HidIoctlEnum hidIoctl = e.HidIoctlEnum;

            switch (hidIoctl)
            {
                case HidIoctlEnum.SetFeature:
                    ffbWrapper.SetFeature(buffer);
                    break;

                case HidIoctlEnum.GetFeature:
                    buffer = ffbWrapper.GetFeature(buffer);
                    break;

                case HidIoctlEnum.WriteReport:
                    ffbWrapper.WriteReport(buffer);
                    break;

                case HidIoctlEnum.GetInputReport:
                case HidIoctlEnum.SetOutputReport:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}