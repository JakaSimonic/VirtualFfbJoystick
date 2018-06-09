using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace vHidService
{
    class Program
    {
        static void Main(string[] args)
        {
            var rc = HostFactory.Run(x =>                                   //1
            {
                x.Service<VhidService>(s =>                                   //2
                {
                    s.ConstructUsing(name => new VhidService());                //3
                    s.WhenStarted(tc => tc.Start());                         //4
                    s.WhenStopped(tc => tc.Stop());                          //5
                });
                x.RunAsLocalSystem();                                       //6
                x.UseNLog();
                x.SetDescription("Virtusl jostick");                   //7
                x.SetDisplayName("Nerville");                                  //8
                x.SetServiceName("Nerville");                                  //9
            });                                                             //10

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());  //11
            Environment.ExitCode = exitCode;
        }
    }
}
