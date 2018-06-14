using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using MitsubishiPlc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using VHClibWrapper;

namespace vHidService
{
    public class VhidService
    {
        private System.Timers.Timer timerPlc;
        private System.Timers.Timer timerWebUpdate;
        private System.Timers.Timer timerServiceManager;

        private static FfbWrapper ffbWrapper = new FfbWrapper();
        private static FfbWrapper manualFfbWrapper;
        public static ManualFfb manualFfb;
        private static McProtocol mcProtocol = new McProtocol();
        private static DriverPolling driverPolling;
        private IHubConnectionContext<dynamic> Clients;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private IDisposable _webApplication;
        private bool webUpdate = false;

        public VhidService()
        {
            try
            {
                InitSettings();
                InitFfb();
                InitDriver();
                InitTimers();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "");
            }
        }

        public bool Start()
        {
            try
            {
                driverPolling.StartPolling(Context.Settings.driver.pollPeriod);
            }
            catch (Exception pe)
            {
                logger.Error(pe);
            }
            timerPlc.Start();
            timerServiceManager.Start();
            _webApplication = Microsoft.Owin.Hosting.WebApp.Start<WebStartup>(url: "http://localhost:7331/");
            Clients = GlobalHost.ConnectionManager.GetHubContext<FfbHub>().Clients;
            timerWebUpdate.Start();
            Console.WriteLine("Service started");
            return true;
        }

        public bool Stop()
        {
            timerPlc.Stop();
            driverPolling.Dispose();
            _webApplication.Dispose();
            mcProtocol.Dispose();
            Console.WriteLine("Service stopped");
            return true;
        }

        private void InitSettings()
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(Settings));
            TextReader reader = new StreamReader(@"settings.xml");
            object obj = deserializer.Deserialize(reader);
            Context.Settings = (Settings)obj;
            reader.Close();
        }

        private void InitTimers()
        {
            timerPlc = new System.Timers.Timer();
            timerPlc.Interval = Context.Settings.plc.pollPeriod;
            timerPlc.Elapsed += PlcCB;
            timerPlc.AutoReset = true;

            timerWebUpdate = new System.Timers.Timer();
            timerWebUpdate.Interval = 1000;
            timerWebUpdate.Elapsed += (sender, e) => { webUpdate = true; };
            timerWebUpdate.AutoReset = true;

            timerServiceManager = new System.Timers.Timer();
            timerServiceManager.Interval = 1958;
            timerServiceManager.Elapsed += ServiceManager;
            timerServiceManager.AutoReset = true;
        }

        private void PlcCB(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (mcProtocol.UdpConnected)
            {
                try
                {
                    timerPlc.Enabled = false;
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    List<short> joyInput = mcProtocol.ReadWordWise("D", 400, 2).Select(i => (short)i).ToList();
                    byte[] buffer = new byte[35];
                    buffer[0] = 1;
                    buffer[1] = Convert.ToByte(joyInput[0] & 0xFF);
                    buffer[2] = Convert.ToByte(joyInput[0] >> 8 & 0xFF);
                    /*
                    buffer[3] = Convert.ToByte(joyInput[0] & 0xFF);
                    buffer[4] = Convert.ToByte(joyInput[0] >> 8 & 0xFF);
                    buffer[5] = Convert.ToByte(joyInput[0] & 0xFF);
                    buffer[6] = Convert.ToByte(joyInput[0] >> 8 & 0xFF);
                    buffer[7] = Convert.ToByte(joyInput[0] & 0xFF);
                    buffer[8] = Convert.ToByte(joyInput[0] >> 8 & 0xFF);
                    buffer[9] = Convert.ToByte(joyInput[0] & 0xFF);
                    buffer[11] = Convert.ToByte(joyInput[0] >> 8 & 0xFF);
                    buffer[12] = Convert.ToByte(joyInput[0] & 0xFF);
                    buffer[13] = Convert.ToByte(joyInput[0] >> 8 & 0xFF);
                    buffer[14] = Convert.ToByte(joyInput[0] & 0xFF);
                    buffer[16] = Convert.ToByte(joyInput[0] >> 8 & 0xFF);
                    buffer[17] = Convert.ToByte(joyInput[0] & 0xFF);
                    buffer[18] = Convert.ToByte(joyInput[0] >> 8 & 0xFF);
                    buffer[19] = Convert.ToByte(joyInput[0] & 0xFF);
                    buffer[20] = Convert.ToByte(joyInput[0] & 0xFF);
                    buffer[21] = Convert.ToByte(joyInput[0] >> 8 & 0xFF);
                    buffer[22] = Convert.ToByte(joyInput[0] & 0xFF);
                    buffer[23] = Convert.ToByte(joyInput[0] >> 8 & 0xFF);
                    buffer[24] = Convert.ToByte(joyInput[0] & 0xFF);
                    buffer[26] = Convert.ToByte(joyInput[0] >> 8 & 0xFF);
                    buffer[27] = Convert.ToByte(joyInput[0] & 0xFF);
                    buffer[28] = Convert.ToByte(joyInput[0] >> 8 & 0xFF);
                    buffer[29] = Convert.ToByte(joyInput[0] & 0xFF);
                    buffer[30] = Convert.ToByte(joyInput[0] & 0xFF);
                    buffer[31] = Convert.ToByte(joyInput[0] >> 8 & 0xFF);
                    buffer[32] = Convert.ToByte(joyInput[0] & 0xFF);
                    buffer[33] = Convert.ToByte(joyInput[0] >> 8 & 0xFF);
                    buffer[34] = Convert.ToByte(joyInput[0] & 0xFF);
                    buffer[36] = Convert.ToByte(joyInput[0] >> 8 & 0xFF);
                    buffer[37] = Convert.ToByte(joyInput[0] & 0xFF);
                    buffer[38] = Convert.ToByte(joyInput[0] >> 8 & 0xFF);
                    buffer[39] = Convert.ToByte(joyInput[0] & 0xFF);
                    */
                    driverPolling.SendReadReport(buffer);

                    List<double> ffbInput = joyInput.Select(i => (double)i).ToList();
                    List<int> res = ffbWrapper.GetForces(new Ffb.JOYSTICK_INPUT { axesPositions = ffbInput, pressedButtonOffsets = new List<int>() }).Select(i => (int)i).ToList();
                    List<int> resMan = manualFfbWrapper.GetForces(new Ffb.JOYSTICK_INPUT { axesPositions = ffbInput, pressedButtonOffsets = new List<int>() }).Select(i => (int)i).ToList();
                    int torque = res.Zip(resMan, (u, z) => u + z).ToList()[0];

                    mcProtocol.WriteWordWise(new int[] { torque }, "D", 300);
                    Console.WriteLine("{0}    {1}", torque, joyInput[0]);

                    if (webUpdate)
                    {
                        webUpdate = false;
                        Clients.All.sendToAll(new MonitorData { wheelPosition = (int)joyInput[0], torque = torque });
                    }
                }
                catch (Exception exc)
                {
                    logger.Error(exc.ToString());
                    mcProtocol.UdpDisconnect();
                }
                timerPlc.Enabled = true;
            }
        }

        private void ServiceManager(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!mcProtocol.UdpConnected)
            {
                mcProtocol.UdpConnect(Context.Settings.plc.ipaddress, Context.Settings.plc.port);
            }

            if (!driverPolling.Connected)
            {
                try
                {
                    driverPolling.StartPolling(Context.Settings.driver.pollPeriod);
                }
                catch (Exception pe)
                {
                    logger.Error(pe);
                }
            }
        }

        private static void Serialize(Settings details)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            using (TextWriter writer = new StreamWriter(@"settings.xml"))
            {
                serializer.Serialize(writer, details);
            }
        }

        private void InitFfb()
        {
            manualFfbWrapper = new FfbWrapper();
            manualFfb = new ManualFfb(manualFfbWrapper);
        }

        private void InitDriver()
        {
            driverPolling = new DriverPolling();
            driverPolling.HidEvent += DriverPoll;
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