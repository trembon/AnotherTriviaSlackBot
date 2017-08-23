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

        private const int FIVE_MINUTEs_IN_MS = 1000 * 5;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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
            logger.Info("Starting service.");

            // preload database info on start
            DAL.TriviaDB.GetQuestionCount();
            DAL.TriviaDB.GetCategories();

            // connect to the slack server
            Connect();

            // create a time to run every 5th minute
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = FIVE_MINUTEs_IN_MS;
            timer.Enabled = true;

            logger.Info("Service started.");
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            logger.Info("Stopping service.");

            if (client.IsConnected)
                client.CloseSocket();

            logger.Info("Service stopped.");
        }
        #endregion

        /// <summary>
        /// Connects to slack.
        /// </summary>
        private void Connect()
        {
            if (client == null)
            {
                client = new SlackSocketClient(configuration.SlackAuthToken);
                client.OnMessageReceived += Client_OnMessageReceived;
            }

            client.Connect((connected) =>
            {
                if (connected.ok)
                {
                    logger.Info("Connected to slack.");

                    // set the user id of the bot in the message handler
                    messageHandler.SetBotID(connected.self.id);

                    // load the channel id from the configured name of the channel
                    channelId = client.GetChannelID(configuration.ChannelName);
                }
                else
                {
                    logger.Error($"Failed to send message with error: {connected.error}");
                }
            },
            () =>
            {
                logger.Info("Socket connected to slack.");
            });
        }

        /// <summary>
        /// Sends the message to the configured channel.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SendMessage(string message)
        {
            if (client == null)
                return;

            if (string.IsNullOrWhiteSpace(message))
                return;

            client.SendMessage(msgSent =>
            {
                if (msgSent.ok)
                {
                    logger.Info($"Message was sent to channel with id '{channelId}' with text: {message}");
                }
                else
                {
                    logger.Error($"Failed to send message with error code: {msgSent.error.code}, message: {msgSent.error.msg}");
                }
            }, channelId, message);
        }

        /// <summary>
        /// Handles the Elapsed event of the Timer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // check if the client is connected to slack
            if (client != null && !client.IsConnected)
            {
                logger.Info("Client is not connected, trying to reconnect");

                // clean up old client
                client.OnMessageReceived -= Client_OnMessageReceived;
                client.CloseSocket();
                client = null;

                // if the client is not connect, try to reconnect it
                Connect();
            }
        }

        /// <summary>
        /// Handles the OnMessageReceived event on from the slack server.
        /// </summary>
        /// <param name="message">The message.</param>
        private void Client_OnMessageReceived(NewMessage message)
        {
            // checks if the message is received in the configured channel
            if (message.channel.Equals(channelId, StringComparison.OrdinalIgnoreCase))
            {
                logger.Info($"Message was received in configured channel from user {message.user} with text: {message.text}");
                messageHandler.HandleMessage(message.text, message.user);
            }
            else
            {
                logger.Info($"Message was received in unknown channel ({message.channel}) from user {message.user} with text: {message.text}");
            }
        }
    }
}
