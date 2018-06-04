using Owin;
using System.Web.Http;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace vHidService
{
    internal class WebStartup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        //
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.UseCors(CorsOptions.AllowAll);

            appBuilder.MapSignalR();

            appBuilder.Map("/api", api =>
            {
                var config = new HttpConfiguration();

                config.MapHttpAttributeRoutes();
                api.UseWebApi(config);
            });

            var fileSystem = new PhysicalFileSystem(@"..\..\..\www"); //xcopy "$(ProjectDir)plc-app" "$(TargetDir)plc-app\" /Y /E /D

            var options = new FileServerOptions
            {
                EnableDirectoryBrowsing = true,
                FileSystem = fileSystem

            };
            options.StaticFileOptions.ServeUnknownFileTypes = true;
            options.DefaultFilesOptions.DefaultFileNames = new string[] { "index.html" };
            appBuilder.UseFileServer(options);


        }
    }
}