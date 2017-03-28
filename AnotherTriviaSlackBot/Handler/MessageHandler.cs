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
        private string botId;
        private TriviaHandler triviaHandler;
        private Action<string> sendMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandler"/> class.
        /// </summary>
        /// <param name="triviaHandler">The trivia handler.</param>
        /// <param name="sendMessage">The send message.</param>
        public MessageHandler(TriviaHandler triviaHandler, Action<string> sendMessage)
        {
            this.botId = null;
            this.triviaHandler = triviaHandler;
            this.sendMessage = sendMessage;
        }

        /// <summary>
        /// Sets the bot identifier.
        /// </summary>
        /// <param name="botId">The bot identifier.</param>
        public void SetBotID(string botId)
        {
            lock (this.triviaHandler.MainLock)
            {
                this.botId = botId;
            }
        }

        /// <summary>
        /// Handles incomming messages from slack.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="userId">The user identifier.</param>
        public void HandleMessage(string message, string userId)
        {
            lock (this.triviaHandler.MainLock)
            {
                if (botId == null)
                    return;

                // trim the message and check if the message starts with the trivia bot as "tagged" like @triviabot
                message = message.Trim();
                if (message.StartsWith($"<@{botId}>"))
                {
                    // parse out what command is sent to the bot
                    string cmd = message.Substring($"<@{botId}>".Length).Trim().ToLower();

                    // check if there are any parameters to the command
                    string[] parameters = new string[0];
                    if(cmd.Contains(' '))
                    {
                        parameters = cmd.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        cmd = parameters[0];
                        parameters = parameters.Skip(1).ToArray();
                    }
                        
                    switch (cmd)
                    {
                        // default text to show the help command
                        case "":
                            sendMessage($"Yes? Just.. just.. here, do this instead: <@{botId}> help");
                            break;

                        // the help, shows what commands are available
                        case "help":
                            sendMessage(
                                $"Don't you think I'm smart bot with {TriviaDB.GetQuestionCount()} questions and understand these commands?\n\n" +
                                "start <category>: Starts a new trivia round for the specified category, if no category is specified the default category is selected.\n" +
                                "stop: Stops the current trivia\n" +
                                "categories: List all available categories\n" +
                                "badquestion X: Mark question X (number) as a bad question, so it will not be asked again\n" +
                                $"\nExample: <@{botId}> start"
                            );
                            break;
                        
                        // starts a new trivia
                        case "start":
                            string categoryName = null;
                            if (parameters.Length > 0)
                                categoryName = parameters[0];

                            triviaHandler.Start(categoryName);
                            break;
                        
                        // stops an ongoing trivia
                        case "stop":
                            triviaHandler.Cancel();
                            break;
                        
                        // shows all available categories
                        case "categories":
                            var categories = TriviaDB.GetCategories();

                            StringBuilder categoriesString = new StringBuilder();
                            categoriesString.Append($"When I was looking I found the following {categories.Count} categories for you to choose from:");
                            foreach(var category in categories)
                                categoriesString.Append($"\n{category.Key} ({category.Value} questions)");

                            sendMessage(categoriesString.ToString());
                            break;

                        // marks a question as bad
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
                        
                        // default that it isnt a known command
                        default:
                            sendMessage("Do I look like someone that understand that jibberish?");
                            break;
                    }
                }
                else
                {
                    // if message wasnt a command, send it to the trivia handler as an answer
                    triviaHandler.ReceiveAnswer(message, userId);
                }
            }
        }
    }
}
