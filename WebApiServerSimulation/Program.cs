using Min_Helpers;
using Min_Helpers.LogHelper;
using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace WebApiServerSimulation
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");

            ConsoleHelper.Initialize();

            Log log = new Log();
            log.Initialize(new FileInfo($"{AppDomain.CurrentDomain.BaseDirectory}log4net.config"));

            try
            {
                ConsoleHelper.Initialize();

                ConsoleHelper.Write("Listen Port: ", ConsoleHelper.EMode.question);
                int port = Convert.ToInt32(Console.ReadLine());
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
