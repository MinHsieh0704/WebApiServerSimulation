using Min_Helpers;
using Min_Helpers.LogHelper;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace WebApiServerSimulation
{
    public class Program
    {
        public static Log log { get; set; }

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");

            ConsoleHelper.Initialize();

            log = new Log();
            log.Initialize(new FileInfo($"{AppDomain.CurrentDomain.BaseDirectory}log4net.config"));

            try
            {
                ConsoleHelper.Initialize();

                ConsoleHelper.Write("Listen Port: ", ConsoleHelper.EMode.question);
                int port = Convert.ToInt32(Console.ReadLine());
                log.Info($"Listen Port: {port}");   

                HttpSelfHostConfiguration config = new HttpSelfHostConfiguration($"http://127.0.0.1:{port}");
                //config.Routes.MapHttpRoute(
                //    name: "Api",
                //    routeTemplate: "{controller}/{action}/{id}",
                //    defaults: new { id = RouteParameter.Optional }
                //);

                config.Routes.MapHttpRoute(
                    name: "Default",
                    routeTemplate: "{*url}",
                    defaults: new { controller = "Index", action = "Handle" }
                );

                var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
                config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

                using (HttpSelfHostServer httpServer = new HttpSelfHostServer(config))
                {
                    httpServer.OpenAsync().Wait();

                    while (true)
                    {
                        string line = Console.ReadLine();
                        if (line == "q" || line == "quit" || line == "bye" || line == "exit") break;
                    }

                    httpServer.CloseAsync().Wait();
                }
            }
            catch (Exception ex)
            {
                ex = ExceptionHelper.GetReal(ex);
                ConsoleHelper.WriteLine($"{ex.Message}", ConsoleHelper.EMode.error);
                log.Error(ex);
            }
            finally
            {
                ConsoleHelper.WriteLine("End", ConsoleHelper.EMode.info);
                log.Info("End");
                Console.ReadKey();

                Environment.Exit(0);
            }
        }
    }
}
