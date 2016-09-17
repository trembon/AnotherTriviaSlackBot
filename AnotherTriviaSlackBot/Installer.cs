﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AnotherTriviaSlackBot
{
    [System.ComponentModel.RunInstaller(true)]
    public class Installer : System.Configuration.Install.Installer
    {
        #region Private fields
        private ServiceProcessInstaller process;
        private ServiceInstaller service;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Installer"/> class.
        /// </summary>
        public Installer()
        {
            // create the process installer
            process = new ServiceProcessInstaller();
            process.Account = ServiceAccount.LocalSystem;

            // create the service installer
            service = new ServiceInstaller();
            service.StartType = ServiceStartMode.Automatic;
            service.ServiceName = Program.SERVICE_NAME;
            service.Description = "A Trivia Slack bot that will keep your Slack friends entertained!";

            // install the process and the service
            Installers.Add(process);
            Installers.Add(service);
        }
        #endregion
    }
}