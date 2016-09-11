using AnotherTriviaSlackBot.Configuration;
using AnotherTriviaSlackBot.Extensions;
using AnotherTriviaSlackBot.Handler;
using SlackAPI;
using SlackAPI.WebSocketMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotherTriviaSlackBot
{
    public class TriviaService
    {
        private SlackSocketClient client;
        private MessageHandler messageHandler;
        private TriviaHandler triviaHandler;

        private MainConfiguration configuration;

        private string channelId;

        public TriviaService()
        {
            configuration = MainConfiguration.Load();

            triviaHandler = new TriviaHandler(configuration, SendMessage);
            messageHandler = new MessageHandler(triviaHandler);

            client = new SlackSocketClient(configuration.SlackAuthToken);
            client.OnMessageReceived += Client_OnMessageReceived;
        }

        private void Client_OnMessageReceived(NewMessage message)
        {
            if(message.channel.Equals(channelId, StringComparison.InvariantCultureIgnoreCase))
            {
                messageHandler.HandleMessage(message.text, message.user);
            }
        }

        private void SendMessage(string message)
        {
            client.SendMessage(msgSent => { /* message sent */ }, channelId, message);
        }

        public void Start()
        {
            client.Connect((connected) => {
                messageHandler.SetBotID(connected.self.id);
                channelId = client.GetChannelID(configuration.ChannelName);
            }, () => { /* message sent */ });
        }

        public void Stop()
        {
            if (client.IsConnected)
                client.CloseSocket();
        }
    }
}
