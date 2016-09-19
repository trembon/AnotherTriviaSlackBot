using AnotherTriviaSlackBot.DAL;
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
        private Action<string> sendMessage;

        public MessageHandler(TriviaHandler triviaHandler, Action<string> sendMessage)
        {
            this.botId = null;
            this.triviaHandler = triviaHandler;
            this.sendMessage = sendMessage;
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

                message = message.Trim();
                if (message.StartsWith($"<@{botId}>"))
                {
                    string cmd = message.Substring($"<@{botId}>".Length).Trim().ToLower();

                    string[] parameters = new string[0];
                    if(cmd.Contains(' '))
                    {
                        parameters = cmd.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        cmd = parameters[0];
                        parameters = parameters.Skip(1).ToArray();
                    }
                        
                    switch (cmd)
                    {
                        case "":
                            sendMessage($"Yes? Just.. just.. here, do this instead: <@{botId}> help");
                            break;

                        case "help":
                            sendMessage(
                                $"Don't you think I'm smart bot with {TriviaDB.GetQuestionCount()} questions and understand these commands?\n\n" +
                                "start: Starts a new trivia round.\n" +
                                "stop: Stops the current trivia\n" +
                                "badquestion X: Mark question X (number) as a bad question, so it will not be asked again\n" +
                                $"\nExample: <@{botId}> start"
                            );
                            break;

                        case "start":
                            triviaHandler.Start();
                            break;

                        case "stop":
                            triviaHandler.Cancel();
                            break;

                        case "badquestion":
                            if (parameters.Length >= 1)
                            {
                                int number;
                                if (int.TryParse(parameters[0], out number) && number > 0 && triviaHandler.CurrentQuestionCount >= number)
                                {
                                    string questionId = triviaHandler.GetQuestionIDByNumber(number);
                                    TriviaDB.SetQuestionAsBad(questionId, userId);
                                    sendMessage("Ok, got it! So you don't know about that...");
                                }
                                else
                                {
                                    sendMessage($"'{parameters[0]}' is not a question number...");
                                }
                            }
                            else
                            {
                                sendMessage("Ain't you missing something? Like.. the question number?");
                            }
                            break;

                        default:
                            sendMessage("Do I look like someone that understand that jibberish?");
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
