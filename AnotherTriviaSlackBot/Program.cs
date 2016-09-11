using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AnotherTriviaSlackBot
{
    public class Program : ServiceBase
    {
        private static readonly Logger log = LogManager.GetLogger("Service");

        private TriviaService triviaService;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // create the new base service
            Program service = new Program();

            // check mode on the application, cmd or service
            if (Environment.UserInteractive)
            {
                // start the service in a cmd window
                service.OnStart(args);
                Console.WriteLine("Press any key to stop the program.");
                Console.Read();
                service.OnStop();
            }
            else
            {
                // run the service as a windows service
                ServiceBase.Run(service);
            }
        }

        /// <summary>
        /// Handles the UnhandledException event of the CurrentDomain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            log.Error(e.ExceptionObject as Exception, "Uncought exception.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Program"/> class.
        /// </summary>
        public Program()
        {
            ServiceName = "Trivia - Slack Bot";
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            // log
            log.Info("Service is starting.");

            // clear the old service
            if (triviaService != null)
                triviaService.Stop();

            // create a new service
            triviaService = new TriviaService();

            // start the service
            triviaService.Start();

            // log
            log.Info("Service has started.");
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            // log
            log.Info("Service is stopping.");

            // stop and empty the service
            if (triviaService != null)
            {
                triviaService.Stop();
                triviaService = null;
            }

            // log
            log.Info("Service has stopped.");
        }
    }
}
