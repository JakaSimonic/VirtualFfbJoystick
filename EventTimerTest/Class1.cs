using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventTimerTest
{
    class Class1
    {
        public static void SampleEventHandler(object src, HidEventArg mea)
        {
            Console.WriteLine(Encoding.ASCII.GetString(mea.buffer));
            string msg = "Hello back";
            mea.buffer = Encoding.ASCII.GetBytes(msg);
        }
    }
}
