using AnotherTriviaSlackBot.Configuration;
using AnotherTriviaSlackBot.Extensions;
using AnotherTriviaSlackBot.Handler;
using NLog;
using SlackAPI;
using SlackAPI.WebSocketMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AnotherTriviaSlackBot
{
    public class Service : ServiceBase
    {
        public const string SERVICE_NAME = "Trivia - Slack Bot";
        public const string SERVICE_DESC = "A Trivia Slack bot that will keep your Slack friends entertained!";

        private static readonly Logger log = LogManager.GetLogger("Service");

        private SlackSocketClient client;
        private MessageHandler messageHandler;
        private TriviaHandler triviaHandler;

        private MainConfiguration configuration;

        private string channelId;

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        public Service()
        {
            ServiceName = SERVICE_NAME;

            configuration = MainConfiguration.Load();

            triviaHandler = new TriviaHandler(configuration, SendMessage);
            messageHandler = new MessageHandler(triviaHandler, SendMessage);

            client = new SlackSocketClient(configuration.SlackAuthToken);
            client.OnMessageReceived += Client_OnMessageReceived;
        }

        #region Service methods
        /// <summary>
        /// Starts the service with the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public void Start(string[] args)
        {
            this.OnStart(args);
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            // preload database info on start
            DAL.TriviaDB.GetQuestionCount();
            DAL.TriviaDB.GetCategories();

            // connect to the slack server
            Connect();

            // create a time to run every 5th minute
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 1000 * 60 * 5; // 5 minutes
            timer.Enabled = true;
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            if (client.IsConnected)
                client.CloseSocket();
        }
        #endregion

        private void Connect()
        {
            client.Connect((connected) => {
                messageHandler.SetBotID(connected.self.id);
                channelId = client.GetChannelID(configuration.ChannelName);
            }, () => { /* message sent */ });
        }

        private void SendMessage(string message)
        {
            client.SendMessage(msgSent => { /* message sent */ }, channelId, message);
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!client.IsConnected)
            {
                log.Info("Client is not connected, trying to reconnect");

                // if the client is not connect, try to reconnect it
                Connect();
            }
        }

        private void Client_OnMessageReceived(NewMessage message)
        {
            if(message.channel.Equals(channelId, StringComparison.InvariantCultureIgnoreCase))
            {
                messageHandler.HandleMessage(message.text, message.user);
            }
        }
    }
}
