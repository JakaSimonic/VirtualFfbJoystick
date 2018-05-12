using MitsubishiPlc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using Topshelf;
using VHClibWrapper;
using System.Timers;

namespace vHidService
{
    public class VhidService : ServiceControl
    {
        private System.Timers.Timer timerPlc;

        private static FfbWrapper ffbWrapper;
        private static McProtocol mcProtocol=new McProtocol();
        private static DriverPolling driverPolling;

        private Settings settings;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public VhidService()
        {
            try
            {
                InitSettings();
                InitFfb();
                InitDriver();
                InitPlc();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "");
            }
        }

        public bool Start(HostControl hostControl)
        {
            Console.WriteLine("Service started");
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            Console.WriteLine("Service stopped");
            return true;
        }

        private void InitSettings()
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(Settings));
            TextReader reader = new StreamReader(@"settings.xml");
            object obj = deserializer.Deserialize(reader);
            settings = (Settings)obj;
            reader.Close();
        }

        private void InitPlc()
        {

            timerPlc = new System.Timers.Timer();
            timerPlc.Interval = settings.plc.pollPeriod;

            // Hook up the Elapsed event for the timer.
            timerPlc.Elapsed += PlcCB;

            // Have the timer fire repeated events (true is the default)
            timerPlc.AutoReset = true;

            // Start the timer
            timerPlc.Start();

            
        }

        void PlcCB(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(!mcProtocol.UdpConnected)
            {
                mcProtocol.UdpConnect(settings.plc.ipaddress, settings.plc.port);
            }
            List<double> joyInput = mcProtocol.ReadWordWise("D", 400, 1).Select(i => (double)(short)i).ToList();
            int[] res = ffbWrapper.GetForces(new Ffb.JOYSTICK_INPUT { axesPositions = joyInput, pressedButtonOffsets = new List<int>() }).Select(i => (int)i).ToArray();
            mcProtocol.WriteWordWise(res, "D", 300);
            Console.WriteLine("{0}", res[0]);

        }
        static public void Serialize(Settings details)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            using (TextWriter writer = new StreamWriter(@"settings.xml"))
            {
                serializer.Serialize(writer, details);
            }
        }

        private void InitFfb()
        {
            ffbWrapper = new FfbWrapper();
        }

        private void InitDriver()
        {
            driverPolling = new DriverPolling();
            driverPolling.HidEvent += DriverPoll;
            driverPolling.StartPolling(settings.driver.pollPeriod);
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