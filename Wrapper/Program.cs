using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHClibWrapper
{
    class Program
    {
        private static void Main(string[] args)
        {
            bool result = NativeMethods.OpenFile();
            uint res = NativeMethods.TestWrite("Work fucker");
            Console.WriteLine("Return {0} ", NativeMethods.TestRead());
            result = NativeMethods.CloseFile();
            Console.Read();
        }
    }
}
