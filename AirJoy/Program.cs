using System;
using System.Collections.Generic;
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

            while (true)
            {
                List<double> res = ffbWrapper.GetForces(new Ffb.JOYSTICK_INPUT { axesPositions = new List<double>() { 0, 0 }, pressedButtonOffsets = new List<int>() });
                //Console.WriteLine("{0}", res[0]);
                System.Threading.Thread.Sleep(100);
            }
        }

        private static void DriverPoll(object sender, HidEventArg e)
        {
            Console.WriteLine(BitConverter.ToString(e.buffer));
            HidIoctlEnum hidIoctl = e.HidIoctlEnum;

            switch (hidIoctl)
            {
                case HidIoctlEnum.SetFeature:
                    ffbWrapper.SetFeature(e.buffer);
                    break;

                case HidIoctlEnum.GetFeature:
                    e.buffer = ffbWrapper.GetFeature(e.buffer);
                    break;

                case HidIoctlEnum.WriteReport:
                    ffbWrapper.WriteReport(e.buffer);
                    break;

                case HidIoctlEnum.GetInputReport:
                case HidIoctlEnum.SetOutputReport:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}