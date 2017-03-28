using AnotherTriviaSlackBot.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AnotherTriviaSlackBot
{
    public class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static Service service;

        /// <summary>
        /// Start of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            // set up unhandled loggin
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // check if a custom settings.json file should be used from the arguments
            if (args != null)
                MainConfiguration.SettingsFile = args.Where(arg => arg.StartsWith("-config=", StringComparison.OrdinalIgnoreCase)).Select(c => c.Substring(8)).FirstOrDefault();

            // create the new base service
            service = new Service();

            // check mode on the application (windows service or console application)
            if (Environment.UserInteractive)
            {
                // check if the application should be started directly or show the 'menu' to the user
                if (args != null && args.Any(arg => arg.Equals("-console", StringComparison.OrdinalIgnoreCase)))
                {
                    // run the service as a console app, if -console argument is specified
                    RunAsConsole(args.Skip(1).ToArray());
                }
                else
                {
                    // handle input of the user
                    HandleConsoleInput(args);
                }
            }
            else
            {
                // run the service as a windows service
                ServiceBase.Run(service);
            }
        }

        /// <summary>
        /// Starts the service as a console application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void RunAsConsole(string[] args)
        {
            // clear the console output, just in case
            Console.Clear();

            // start the service as a console application
            service.Start(args);
            Console.WriteLine("Press <ENTER> to stop the program.");

            // accept to send message from the console, if empty message, close the application
            string message = "";
            do
            {
                message = Console.ReadLine();
                service.SendMessage(message);
            } while (!string.IsNullOrWhiteSpace(message));

            service.Stop();

            // exit the application when done
            Environment.Exit(0);
        }

        #region Console 'menu' handlers
        /// <summary>
        /// Handles the console input for the 'menu'.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void HandleConsoleInput(string[] args)
        {
            while (true)
            {
                bool isInstalled = false;
                ServiceController windowsService = new ServiceController(Service.SERVICE_NAME);
                try { isInstalled = windowsService.DisplayName == Service.SERVICE_NAME; } catch { }

                Console.WriteLine("Select your choice:");
                Console.WriteLine($"[s] Start '{Service.SERVICE_NAME}' as a console application.");
                Console.WriteLine("");

                if (isInstalled)
                {
                    Console.WriteLine($"[u] Uninstall the '{Service.SERVICE_NAME}' service.");
                    if (windowsService.Status == ServiceControllerStatus.Running)
                    {
                        Console.WriteLine($"[2] Stop the '{Service.SERVICE_NAME}' service.");
                        Console.WriteLine($"[3] Restart the '{Service.SERVICE_NAME}' service.");
                    }
                    else
                    {
                        Console.WriteLine($"[1] Start the '{Service.SERVICE_NAME}' service.");
                    }
                }
                else
                {
                    Console.WriteLine($"[i] Install '{Service.SERVICE_NAME}' as a windows service.");
                }

                Console.WriteLine("");
                Console.WriteLine("[q] Exit.");
                Console.WriteLine();

                Console.Write("Your input: ");
                ConsoleKeyInfo input = Console.ReadKey();

                Console.WriteLine("");
                Console.WriteLine("");

                switch (input.KeyChar)
                {
                    case 's':
                        RunAsConsole(args);
                        break;

                    case 'q':
                        Environment.Exit(0);
                        break;

                    case 'i':
                        Console.WriteLine("Installing the service.");

                        HideOutput(() =>
                        {
                            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                        },
                        () => Console.WriteLine("Installing the service - complete."),
                        () => Console.WriteLine("An error occured, verify that you are running as an administrator"));
                        break;

                    case 'u':
                        Console.WriteLine("Uninstalling the service.");

                        HideOutput(() =>
                        {
                            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                        },
                        () => Console.WriteLine("Uninstalling the service - complete."),
                        () => Console.WriteLine("An error occured, verify that you are running as an administrator"));
                        break;

                    case '1':
                        StartService(windowsService);
                        break;

                    case '2':
                        StopService(windowsService);
                        break;

                    case '3':
                        StopService(windowsService);
                        StartService(windowsService);
                        break;

                    default:
                        Console.WriteLine("Invalid input.");
                        break;
                }
                
                Console.WriteLine("");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
                Console.Clear();
            }
        }

        /// <summary>
        /// Hides the output to the console.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="onSuccess">The on success.</param>
        /// <param name="onError">The on error.</param>
        private static void HideOutput(Action action, Action onSuccess = null, Action onError = null)
        {
            // save the original output stream
            var originalOutput = Console.Out;

            try
            {
                // set an empty out writer for the console, so no info should be shown
                Console.SetOut(TextWriter.Null);

                // perform the specified action
                action();

                // restore the original output writer
                Console.SetOut(originalOutput);

                // on success, trigger success action
                onSuccess?.Invoke();
            }
            catch(Exception ex)
            {
                // if anything fails, log, restore output stream and trigger error method
                logger.Error(ex, "Failed to perform action in HideOutput method.");
                Console.SetOut(originalOutput);
                onError?.Invoke();
            }
        }

        /// <summary>
        /// Starts the windows service.
        /// </summary>
        /// <param name="service">The service.</param>
        private static void StartService(ServiceController service)
        {
            try
            {
                Console.WriteLine("Starting the service.");

                // starts the service and waits for it to be running
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running);

                Console.WriteLine("The service has now started.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to start the windows service '{Service.SERVICE_NAME}'.");
                Console.WriteLine("An error occured, verify that you are running as an administrator");
            }
        }

        /// <summary>
        /// Stops the windows service.
        /// </summary>
        /// <param name="service">The service.</param>
        private static void StopService(ServiceController service)
        {
            try
            {
                Console.WriteLine("Stopping the service.");

                // stops the service and waits to be completly stopped
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped);

                Console.WriteLine("The service has now stopped.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to stop the windows service '{Service.SERVICE_NAME}'.");
                Console.WriteLine("An error occured, verify that you are running as an administrator");
            }
        }
        #endregion


        /// <summary>
        /// Handles the UnhandledException event of the CurrentDomain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logger.Error(e.ExceptionObject as Exception, "Uncought exception.");
        }
    }
}
