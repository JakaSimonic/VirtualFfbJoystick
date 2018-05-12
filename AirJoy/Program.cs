using System;
using System.Collections.Generic;
using VHClibWrapper;
using System.Linq;
using System.Threading;
using MitsubishiPlc;
namespace AirJoy
{
    internal class Program
    {
        private static FfbWrapper ffbWrapper;
        private static McProtocol mcProtocol;

        private static void Main(string[] args)
        {
            mcProtocol = new McProtocol();
            mcProtocol.UdpConnect("192.168.33.39", 1028);

            ffbWrapper = new FfbWrapper();
            DriverPolling driverPoll = new DriverPolling();
            driverPoll.HidEvent += DriverPoll;

            driverPoll.StartPolling(30);

            while (true)
            {
                List<double> joyInput = mcProtocol.ReadWordWise("D", 400, 1).Select(i => (double)i).ToList();
                int[] res = ffbWrapper.GetForces(new Ffb.JOYSTICK_INPUT { axesPositions = joyInput, pressedButtonOffsets = new List<int>() }).Select(i => (int)i).ToArray();
                mcProtocol.WriteWordWise(res, "D", 300);
                Console.WriteLine("{0}", res[0]);
                System.Threading.Thread.Sleep(100);
            }
        }

        private static void DriverPoll(object sender, HidEventArg e)
        {
            
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