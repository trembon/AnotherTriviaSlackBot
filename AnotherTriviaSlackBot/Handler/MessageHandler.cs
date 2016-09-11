using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotherTriviaSlackBot.Handler
{
    public class MessageHandler
    {
        private object handleLock = new object();

        private string botId;
        private TriviaHandler triviaHandler;

        public MessageHandler(TriviaHandler triviaHandler)
        {
            this.botId = null;
            this.triviaHandler = triviaHandler;
        }

        public void SetBotID(string botId)
        {
            lock (handleLock)
            {
                this.botId = botId;
            }
        }

        public void HandleMessage(string message, string userId)
        {
            lock (handleLock)
            {
                if (botId == null)
                    return;

                if (message.StartsWith($"<@{botId}> "))
                {
                    string cmd = message.Substring($"<@{botId}> ".Length).ToLower();
                    switch (cmd)
                    {
                        case "start":
                            triviaHandler.Start();
                            break;

                        case "stop":
                            triviaHandler.Cancel();
                            break;
                    }
                }
                else
                {
                    triviaHandler.ReceiveAnswer(message, userId);
                }
            }
        }
    }
}
