using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace EventTimerTest
{
    class Program
    {
        private static Timer pollTimer;
        public static event EventHandler<HidEventArg> ivent;

        static void Main(string[] args)
        {
            Class1 cla = new Class1();
            ivent += Class1.SampleEventHandler;

            pollTimer = new System.Timers.Timer();
            pollTimer.Interval = 1000;

            // Hook up the Elapsed event for the timer. 
            pollTimer.Elapsed += OnPollEvent;

            // Have the timer fire repeated events (true is the default)
            pollTimer.AutoReset = true;

            // Start the timer
            pollTimer.Start();

            //Console.WriteLine("Press the Enter key to exit the program at any time... ");
            //Console.ReadLine();

        }

        private static void OnPollEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            HidEventArg he = new HidEventArg();
            string msg = "Hello";
            he.buffer = Encoding.ASCII.GetBytes(msg);
            ivent(null, he);
            Console.WriteLine(Encoding.ASCII.GetString(he.buffer));

        }

    }

    public class HidEventArg : EventArgs
    {
        public byte[] buffer;
    }

}
