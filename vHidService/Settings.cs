using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vHidService
{
    public class Settings
    {
        public Plc plc;
        public Driver driver;
    }

    public class Plc
    {
        public string ipaddress;
        public int port;
        public long pollPeriod;
    }
    public class Driver
    {
        public int pollPeriod;
    }

}
