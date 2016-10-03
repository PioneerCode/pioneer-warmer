using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using Autofac;
using Newtonsoft.Json;
using NLog;
using Pioneer.Warmer.Services;

namespace Pioneer.Warmer.Service
{
    internal static class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Build autofac container
        /// </summary>
        private static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(BuildConfig());
            builder.RegisterType<Warmer>().As<IWarmer>();
            builder.RegisterType<NotificationService>().As<INotificationService>();
            builder.RegisterType<ValidationService>().As<IValidationService>();
            builder.RegisterType<Service>().As<Service>();

            // Build up autofac container
            return builder.Build();
        }

        /// <summary>
        /// Read configuration file
        /// </summary>
        private static Config BuildConfig()
        {
            var config = File.ReadAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\config.json");
            return JsonConvert.DeserializeObject<Config>(config);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            var container = BuildContainer();

            // Log unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => Logger.Error(args.ExceptionObject.ToString(), args.ExceptionObject);

            var servicesToRun = new ServiceBase[]
                                {
                                    container.Resolve<Service>()
                                };

            if (!Debugger.IsAttached)
            {
                ServiceBase.Run(servicesToRun);
            }
            else
            {
                RunInteractive(servicesToRun);
            }
        }

        /// <summary>
        /// Run service in console.
        /// Aids in debugging. 
        /// </summary>
        private static void RunInteractive(ServiceBase[] servicesToRun)
        {
            Console.WriteLine("Service running in interactive mode.");
            Console.WriteLine();

            var onStartMethod = typeof(ServiceBase).GetMethod("OnStart", BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var service in servicesToRun)
            {
                Console.Write("Starting {0}...", service.ServiceName);
                onStartMethod.Invoke(service, new object[] { new string[] { } });
                Console.Write("Started");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to stop the services and end the process...");
            Console.WriteLine();
            Console.ReadKey();
            Console.WriteLine();

            var onStopMethod = typeof(ServiceBase).GetMethod("OnStop", BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var service in servicesToRun)
            {
                Console.Write("Stopping {0}...", service.ServiceName);
                onStopMethod.Invoke(service, null);
                Console.WriteLine("Stopped");
            }

            Console.WriteLine("All services stopped.");

            // Keep the console alive for a second to allow the user to see the message.
            Thread.Sleep(1000);
        }
    }
}
