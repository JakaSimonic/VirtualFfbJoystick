using System;

namespace TestWrapper
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            NativeMethods.OpenFile();
            NativeMethods.TestWrite("Work fucker");
            Console.WriteLine("Return {0} ", NativeMethods.TestRead());
            NativeMethods.CloseFile();
        }
    }
}