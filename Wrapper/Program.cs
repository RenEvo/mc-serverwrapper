using System;
using System.IO;
using System.ServiceProcess;
using Microsoft.Practices.Unity;
using Wrapper.Unity;

namespace Wrapper
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                Bootstrapper.Initialize();

                Environment.CurrentDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location) ?? "./";
                Directory.CreateDirectory("./Logs/");

                if (Environment.UserInteractive)
                {
                    NativeMethods.AllocConsole();

                    Console.Title = "Minecraft Server Launcher";
                    Console.WriteLine("Starting in console mode");

                    using (var minecraftService = Bootstrapper.Instance.Container.Resolve<MinecraftService>())
                    {
                        minecraftService.DebugStart(args);

                        Console.ReadLine();

                        minecraftService.DebugStop();
                    }
                }
                else
                {
                    ServiceBase.Run(new ServiceBase[] 
                        { 
                            Bootstrapper.Instance.Container.Resolve<MinecraftService>()
                        });
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(string.Format("./Crash.{0}.log", DateTime.UtcNow.Ticks), ex.ToString());
            }
        }
    }
}
