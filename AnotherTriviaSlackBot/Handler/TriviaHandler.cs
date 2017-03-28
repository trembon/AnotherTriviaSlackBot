using AnotherTriviaSlackBot.Configuration;
using AnotherTriviaSlackBot.DAL;
using AnotherTriviaSlackBot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotherTriviaSlackBot.Handler
{
    public class TriviaHandler
    {
        #region Private fields
        private MainConfiguration configuration;
        private Action<string> sendMessage;

        private List<CurrentTriviaQuestion> questions;
        private int currentQuestion;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether a trivia is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a trivia is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets the main lock for the application.
        /// </summary>
        /// <value>
        /// The main lock.
        /// </value>
        public object MainLock { get; } = new object();

        /// <summary>
        /// Gets the current question count.
        /// </summary>
        /// <value>
        /// The current question count.
        /// </value>
        public int CurrentQuestionCount
        {
            get { return currentQuestion + 1; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="TriviaHandler"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="sendMessage">The send message.</param>
        public TriviaHandler(MainConfiguration configuration, Action<string> sendMessage)
        {
            this.configuration = configuration;
            this.sendMessage = sendMessage;
        }
        #endregion

        #region Start/Stop methods
        /// <summary>
        /// Starts a trivia with the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        public void Start(string category)
        {
            // check so there isnt an already active trivia
            if (IsActive)
                return;

            IsActive = true;

            // if category is null, get the default one
            if (String.IsNullOrWhiteSpace(category))
                category = configuration.DefaultCategory;

            // check so the category exists
            var categories = TriviaDB.GetCategories();
            if (!categories.ContainsKey(category))
            {
                sendMessage($"Seriously? The category '{category}' doesn't exist, think before you write!");
                return;
            }

            // show message to channel that a trivia is about to start
            int startDelaySeconds = 1;
            string baseStartMessage = $"A new trivia for '{category}' will start in a moment, just gonna find {configuration.QuestionsPerRound} questions!";
            if (configuration.AnnounceToChannelBeforeStart)
            {
                startDelaySeconds = configuration.StartDelaySecondsAfterAnnounce;
                sendMessage($"<!channel>: {baseStartMessage}");
            }
            else
            {
                sendMessage(baseStartMessage);
            }

            // get some random questions
            currentQuestion = -1;
            questions = TriviaDB.GetRandomQuestions(category, configuration.QuestionsPerRound).Select(q => new CurrentTriviaQuestion(q)).ToList();
               
            // delay depending on configuration, then show the first question
            Task.Delay(startDelaySeconds * 1000).ContinueWith(x => ShowQuestion());
        }

        /// <summary>
        /// Cancels an ongoing trivia.
        /// </summary>
        public void Cancel()
        {
            // check so there really is an active trivia
            if (!IsActive)
            {
                sendMessage("There isn't an active trivia to stop, stupid");
                return;
            }
            
            questions.ForEach(q => q.IsAnswered = true);

            // stop the trivia
            sendMessage("Someone is boring... stopping current trivia");
            IsActive = false;
        }
        #endregion

        #region Question methods
        /// <summary>
        /// Shows the next question.
        /// </summary>
        private void ShowQuestion()
        {
            lock (MainLock)
            {
                currentQuestion++;

                // check if we are done with all the questions or not
                if (currentQuestion < questions.Count)
                {
                    if (!IsActive)
                        return;

                    // get the next question and send to the channel
                    var question = questions[currentQuestion];
                    sendMessage($"*Question {currentQuestion + 1}*: {question.Question.Text}");

                    // queue up the hint and answer methods
                    Task.Delay(configuration.ShowHintAfterSeconds * 1000).ContinueWith(x => ShowHint(question));
                    Task.Delay(configuration.ShowAnswerAfterSeconds * 1000).ContinueWith(x => ShowAnswer(question));
                }
                else
                {
                    // get all users that answerd a question this round
                    Dictionary<string, int> statsAfterRound = questions.Where(x => x.AnswererUserID != null).GroupBy(x => x.AnswererUserID).OrderByDescending(x => x.Count()).ToDictionary(k => k.Key, v => v.Count());

                    // update the database with those player stats
                    TriviaDB.UpdatePlayerStats(statsAfterRound);
                    var top5 = TriviaDB.GetTopFiveUsers();

                    // build a message to show the score of this round and the top 5 best players overall
                    StringBuilder resultMsg = new StringBuilder();
                    resultMsg.Append($"This trivia round is now done, the results are the following:\n");
                    if (statsAfterRound.Count > 0)
                    {
                        resultMsg.Append(String.Join(", ", statsAfterRound.Select(x => $"<@{x.Key}>: {x.Value}")));
                    }
                    else
                    {
                        resultMsg.Append("_No users scored this round, better luck next time!_");
                    }

                    resultMsg.Append("\n\n");

                    resultMsg.Append($"Top 5 total:\n");
                    if (top5.Count > 0)
                    {
                        resultMsg.Append(String.Join(", ", top5.Select(x => $"<@{x.UserID}>: {x.Score}")));
                    }
                    else
                    {
                        resultMsg.Append("_No users have scored yet._");
                    }

                    // send the result message and set trivia as inactive
                    sendMessage(resultMsg.ToString());
                    IsActive = false;
                }
            }
        }

        /// <summary>
        /// Receives the answer.
        /// </summary>
        /// <param name="answer">The answer.</param>
        /// <param name="userId">The user identifier.</param>
        public void ReceiveAnswer(string answer, string userId)
        {
            if (!IsActive)
                return;

            // check if the answer is the correct one for the current question
            var question = questions[currentQuestion];
            if (!question.IsAnswered && question.Question.Answer.Equals(answer, StringComparison.OrdinalIgnoreCase))
            {
                // send that user scored
                sendMessage($"Correct <@{userId}>! The answer is '{question.Question.Answer}'");

                // mark question as answer
                question.IsAnswered = true;
                question.AnswererUserID = userId;

                // continue to next question
                Task.Delay(1000).ContinueWith(x => ShowQuestion());
            }
        }

        /// <summary>
        /// Gets the question identifier by number.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <returns></returns>
        public string GetQuestionIDByNumber(int number)
        {
            return questions[number - 1].Question.ID;
        }

        /// <summary>
        /// Shows the answer.
        /// </summary>
        /// <param name="currentQuestion">The current question.</param>
        private void ShowAnswer(CurrentTriviaQuestion currentQuestion)
        {
            lock (MainLock)
            {
                if (!IsActive)
                    return;

                // check so the current question is not answered
                if (!currentQuestion.IsAnswered)
                {
                    // show answer and continue to next question
                    sendMessage($"No correct answer, what I was looking for was '{currentQuestion.Question.Answer}'");

                    currentQuestion.IsAnswered = true;

                    Task.Delay(1000).ContinueWith(x => ShowQuestion());
                }
            }
        }

        /// <summary>
        /// Shows the hint.
        /// </summary>
        /// <param name="currentQuestion">The current question.</param>
        private void ShowHint(CurrentTriviaQuestion currentQuestion)
        {
            lock (MainLock)
            {
                if (!IsActive)
                    return;

                // check so the current question is not answered, then show the hint
                if (!currentQuestion.IsAnswered)
                {
                    sendMessage($"*Hint*: {currentQuestion.Question.GenerateHint()}");
                }
            }
        }
        #endregion
    }
}
