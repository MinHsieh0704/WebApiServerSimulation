using Min_Helpers;
using Min_Helpers.LogHelper;
using Min_Helpers.PrintHelper;
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
    public class IAccount
    {
        public string account { get; set; }
        public string password { get; set; }
    }

    public class Program
    {
        public static Print PrintService { get; set; } = null;
        public static Log LogService { get; set; } = null;

        public static IAccount basicAuth { get; set; } = null;

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");

            try
            {
                LogService = new Log();
                PrintService = new Print(LogService);

                LogService.Write("");
                PrintService.Log("App Start", Print.EMode.info);

                PrintService.NewLine();

                PrintService.Write("Is Enable Basic Auth Mode? [y/N] ", Print.EMode.question);
                string readLine = Console.ReadLine();
                bool isEnabledBasicAuth = !(string.IsNullOrEmpty(readLine) || readLine.ToLower() == "n" || readLine.ToLower() == "no");
                if (isEnabledBasicAuth)
                {
                    PrintService.Write("Basic Auth Account: [Admin] ", Print.EMode.question);
                    string account = Console.ReadLine();

                    PrintService.Write("Basic Auth Password: [123456] ", Print.EMode.question);
                    string password = Console.ReadLine();

                    basicAuth = new IAccount()
                    {
                        account = string.IsNullOrEmpty(account) ? "Admin" : account,
                        password = string.IsNullOrEmpty(password) ? "123456" : password
                    };
                }

                PrintService.NewLine();

                PrintService.Write("Listen Port: ", Print.EMode.question);
                int port = Convert.ToInt32(Console.ReadLine());

                PrintService.NewLine();

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
                PrintService.Log($"App Error, {ex.Message}", Print.EMode.error);
            }
            finally
            {
                PrintService.Log("App End", Print.EMode.info);
                Console.ReadKey();

                Environment.Exit(0);
            }
        }
    }
}
